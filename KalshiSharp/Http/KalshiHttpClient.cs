using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using KalshiSharp.Configuration;
using KalshiSharp.Errors;
using KalshiSharp.Observability;
using KalshiSharp.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KalshiSharp.Http;

/// <summary>
/// HTTP client for Kalshi API requests with signing, error handling, and observability.
/// </summary>
public sealed partial class KalshiHttpClient : IKalshiHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<KalshiHttpClient> _logger;
    private readonly KalshiClientMetrics? _metrics;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Header name for request ID correlation.
    /// </summary>
    public const string RequestIdHeader = "X-Request-Id";

    /// <summary>
    /// Headers that should be redacted in logs.
    /// </summary>
    private static readonly HashSet<string> SensitiveHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "KALSHI-ACCESS-KEY",
        "KALSHI-ACCESS-SIGNATURE",
        "Authorization"
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="KalshiHttpClient"/> class.
    /// </summary>
    /// <param name="httpClient">The underlying HTTP client (configured via HttpClientFactory).</param>
    /// <param name="options">The client options.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="metrics">Optional metrics collector.</param>
    public KalshiHttpClient(
        HttpClient httpClient,
        IOptions<KalshiClientOptions> options,
        ILogger<KalshiHttpClient> logger,
        KalshiClientMetrics? metrics = null)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _httpClient = httpClient;
        _logger = logger;
        _metrics = metrics;
        _jsonOptions = KalshiJsonOptions.Default;

        var effectiveUri = options.Value.GetEffectiveBaseUri();
        _httpClient.BaseAddress = effectiveUri;
        _httpClient.Timeout = options.Value.Timeout;
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    /// <inheritdoc />
    public async Task<TResponse> SendAsync<TResponse>(KalshiRequest request, CancellationToken cancellationToken = default)
        where TResponse : class
    {
        ArgumentNullException.ThrowIfNull(request);

        var (response, rawContent, requestId) = await SendInternalAsync(request, cancellationToken).ConfigureAwait(false);

        if (string.IsNullOrEmpty(rawContent))
        {
            throw new KalshiException(
                "Expected response body but received empty content",
                response.StatusCode,
                requestId: requestId);
        }

        try
        {
            var result = JsonSerializer.Deserialize<TResponse>(rawContent, _jsonOptions);
            if (result is null)
            {
                throw new KalshiException(
                    "Failed to deserialize response: result was null",
                    response.StatusCode,
                    rawResponse: rawContent,
                    requestId: requestId);
            }
            return result;
        }
        catch (JsonException ex)
        {
            throw new KalshiException(
                $"Failed to deserialize response: {ex.Message}",
                response.StatusCode,
                rawResponse: rawContent,
                requestId: requestId,
                innerException: ex);
        }
    }

    /// <inheritdoc />
    public async Task SendAsync(KalshiRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        await SendInternalAsync(request, cancellationToken).ConfigureAwait(false);
    }

    private async Task<(HttpResponseMessage Response, string? Content, string RequestId)> SendInternalAsync(
        KalshiRequest request,
        CancellationToken cancellationToken)
    {
        var requestId = Guid.NewGuid().ToString("N");
        var path = request.BuildRelativeUri();
        var stopwatch = Stopwatch.StartNew();

        using var activity = KalshiActivitySource.StartHttpRequest(request.Method.Method, path);
        activity?.SetTag(KalshiActivitySource.Tags.RequestId, requestId);

        using var logScope = _logger.BeginScope(new Dictionary<string, object>
        {
            ["RequestId"] = requestId,
            ["Method"] = request.Method.Method,
            ["Path"] = path
        });

        LogSendingRequest(request.Method.Method, path);

        using var httpRequest = CreateHttpRequest(request, path, requestId);

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        }
        catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            LogRequestFailed(stopwatch.ElapsedMilliseconds, ex);
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }

        stopwatch.Stop();
        var statusCode = (int)response.StatusCode;

        activity?.SetTag(KalshiActivitySource.Tags.HttpStatusCode, statusCode);
        _metrics?.RecordRequest(request.Method.Method, path, statusCode, stopwatch.Elapsed);

        LogRequestCompleted(request.Method.Method, path, stopwatch.ElapsedMilliseconds, statusCode);

        string? content = null;
        if (response.Content.Headers.ContentLength != 0)
        {
            content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        }

        if (!response.IsSuccessStatusCode)
        {
            activity?.SetStatus(ActivityStatusCode.Error, $"HTTP {statusCode}");
            HandleErrorResponse(response, content, requestId);
        }

        return (response, content, requestId);
    }

    private HttpRequestMessage CreateHttpRequest(KalshiRequest request, string path, string requestId)
    {
        var httpRequest = new HttpRequestMessage(request.Method, path);
        httpRequest.Headers.TryAddWithoutValidation(RequestIdHeader, requestId);

        if (request.Content is not null)
        {
            var json = JsonSerializer.Serialize(request.Content, request.Content.GetType(), _jsonOptions);
            httpRequest.Content = new StringContent(json, Encoding.UTF8, "application/json");
        }

        return httpRequest;
    }

    private void HandleErrorResponse(HttpResponseMessage response, string? content, string requestId)
    {
        KalshiErrorResponse? errorResponse = null;

        if (!string.IsNullOrEmpty(content))
        {
            try
            {
                errorResponse = JsonSerializer.Deserialize<KalshiErrorResponse>(content, _jsonOptions);
            }
            catch (JsonException)
            {
                // Failed to parse error response, continue with raw content
            }
        }

        var exception = KalshiException.FromResponse(response.StatusCode, errorResponse, content, requestId);

        // Enhance rate limit exception with Retry-After header
        if (exception is KalshiRateLimitException && response.Headers.RetryAfter?.Delta is not null)
        {
            exception = new KalshiRateLimitException(
                exception.Message,
                exception.StatusCode,
                exception.ErrorCode,
                exception.RawResponse,
                requestId,
                response.Headers.RetryAfter.Delta);
        }

        LogErrorResponse(exception);
        throw exception;
    }

    private void LogErrorResponse(KalshiException exception)
    {
        var statusCode = (int)exception.StatusCode;
        var errorCode = exception.ErrorCode ?? "N/A";

        switch (exception)
        {
            case KalshiRateLimitException:
                LogApiErrorWarning(statusCode, errorCode, exception.Message, exception);
                break;
            case KalshiAuthException:
                LogApiErrorError(statusCode, errorCode, exception.Message, exception);
                break;
            case KalshiNotFoundException:
                LogApiErrorDebug(statusCode, errorCode, exception.Message, exception);
                break;
            case KalshiValidationException:
                LogApiErrorWarning(statusCode, errorCode, exception.Message, exception);
                break;
            default:
                LogApiErrorError(statusCode, errorCode, exception.Message, exception);
                break;
        }
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Sending {Method} {Path}")]
    private partial void LogSendingRequest(string method, string path);

    [LoggerMessage(Level = LogLevel.Error, Message = "Request failed after {ElapsedMs}ms")]
    private partial void LogRequestFailed(long elapsedMs, Exception exception);

    [LoggerMessage(Level = LogLevel.Information, Message = "Kalshi API request {Method} {Path} completed in {ElapsedMs}ms with {StatusCode}")]
    private partial void LogRequestCompleted(string method, string path, long elapsedMs, int statusCode);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Kalshi API error: {StatusCode} {ErrorCode} - {Message}")]
    private partial void LogApiErrorDebug(int statusCode, string errorCode, string message, Exception exception);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Kalshi API error: {StatusCode} {ErrorCode} - {Message}")]
    private partial void LogApiErrorWarning(int statusCode, string errorCode, string message, Exception exception);

    [LoggerMessage(Level = LogLevel.Error, Message = "Kalshi API error: {StatusCode} {ErrorCode} - {Message}")]
    private partial void LogApiErrorError(int statusCode, string errorCode, string message, Exception exception);
}
