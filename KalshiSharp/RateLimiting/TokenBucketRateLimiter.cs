using System.Threading.RateLimiting;

namespace KalshiSharp.RateLimiting;

/// <summary>
/// Token bucket rate limiter for Kalshi API requests.
/// Default: 10 requests/second with burst capacity of 20.
/// </summary>
public sealed class TokenBucketRateLimiter : IRateLimiter
{
    private readonly System.Threading.RateLimiting.TokenBucketRateLimiter _limiter;
    private bool _disposed;

    /// <summary>
    /// Default tokens per period (requests per second).
    /// </summary>
    public const int DefaultTokensPerPeriod = 10;

    /// <summary>
    /// Default token limit (burst capacity).
    /// </summary>
    public const int DefaultTokenLimit = 20;

    /// <summary>
    /// Initializes a new instance with default settings (10 req/s, burst 20).
    /// </summary>
    public TokenBucketRateLimiter()
        : this(DefaultTokensPerPeriod, DefaultTokenLimit)
    {
    }

    /// <summary>
    /// Initializes a new instance with custom settings.
    /// </summary>
    /// <param name="tokensPerSecond">Number of tokens replenished per second.</param>
    /// <param name="tokenLimit">Maximum burst capacity.</param>
    public TokenBucketRateLimiter(int tokensPerSecond, int tokenLimit)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(tokensPerSecond);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(tokenLimit);

        _limiter = new System.Threading.RateLimiting.TokenBucketRateLimiter(
            new TokenBucketRateLimiterOptions
            {
                TokenLimit = tokenLimit,
                ReplenishmentPeriod = TimeSpan.FromSeconds(1),
                TokensPerPeriod = tokensPerSecond,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 100,
                AutoReplenishment = true
            });
    }

    /// <inheritdoc />
    public bool IsThrottling => _limiter.GetStatistics()?.CurrentAvailablePermits < 5;

    /// <inheritdoc />
    public async ValueTask WaitAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        using var lease = await _limiter.AcquireAsync(1, cancellationToken).ConfigureAwait(false);

        if (!lease.IsAcquired)
        {
            throw new InvalidOperationException("Failed to acquire rate limiter lease. Queue limit may have been exceeded.");
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _limiter.Dispose();
        _disposed = true;
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }
}
