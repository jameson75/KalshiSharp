using KalshiSharp.Auth;
using Microsoft.Extensions.Logging;

namespace KalshiSharp.Http;

/// <summary>
/// Delegating handler that signs outgoing requests using the Kalshi authentication scheme.
/// </summary>
public sealed partial class SigningDelegatingHandler : DelegatingHandler
{
    private readonly IKalshiRequestSigner _signer;
    private readonly ISystemClock _clock;
    private readonly ILogger<SigningDelegatingHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SigningDelegatingHandler"/> class.
    /// </summary>
    /// <param name="signer">The request signer.</param>
    /// <param name="clock">The system clock.</param>
    /// <param name="logger">The logger.</param>
    public SigningDelegatingHandler(
        IKalshiRequestSigner signer,
        ISystemClock clock,
        ILogger<SigningDelegatingHandler> logger)
    {
        _signer = signer ?? throw new ArgumentNullException(nameof(signer));
        _clock = clock ?? throw new ArgumentNullException(nameof(clock));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        byte[] bodyBytes = [];

        if (request.Content is not null)
        {
            bodyBytes = await request.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
        }

        var timestamp = _clock.UtcNow;
        _signer.Sign(request, bodyBytes, timestamp);

        LogSignedRequest(request.Method, request.RequestUri?.PathAndQuery, timestamp);

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Signed request {Method} {Path} at {Timestamp}")]
    private partial void LogSignedRequest(HttpMethod method, string? path, DateTimeOffset timestamp);
}
