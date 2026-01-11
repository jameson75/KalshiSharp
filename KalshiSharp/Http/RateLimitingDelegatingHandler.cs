using KalshiSharp.RateLimiting;
using Microsoft.Extensions.Logging;

namespace KalshiSharp.Http;

/// <summary>
/// Delegating handler that applies client-side rate limiting before sending requests.
/// </summary>
public sealed partial class RateLimitingDelegatingHandler : DelegatingHandler
{
    private readonly IRateLimiter _rateLimiter;
    private readonly ILogger<RateLimitingDelegatingHandler> _logger;
    private readonly bool _enabled;

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitingDelegatingHandler"/> class.
    /// </summary>
    /// <param name="rateLimiter">The rate limiter.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="enabled">Whether rate limiting is enabled.</param>
    public RateLimitingDelegatingHandler(
        IRateLimiter rateLimiter,
        ILogger<RateLimitingDelegatingHandler> logger,
        bool enabled = true)
    {
        _rateLimiter = rateLimiter ?? throw new ArgumentNullException(nameof(rateLimiter));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _enabled = enabled;
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (_enabled)
        {
            if (_rateLimiter.IsThrottling)
            {
                LogThrottling();
            }

            await _rateLimiter.WaitAsync(cancellationToken).ConfigureAwait(false);
        }

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Rate limiter is throttling, waiting for permit")]
    private partial void LogThrottling();
}
