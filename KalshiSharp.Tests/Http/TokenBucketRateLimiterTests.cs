using FluentAssertions;
using KalshiSharp.RateLimiting;
using Xunit;

namespace KalshiSharp.Tests.Http;

public sealed class TokenBucketRateLimiterTests : IDisposable
{
    private readonly TokenBucketRateLimiter _limiter;

    public TokenBucketRateLimiterTests()
    {
        _limiter = new TokenBucketRateLimiter();
    }

    public void Dispose()
    {
        _limiter.Dispose();
    }

    [Fact]
    public void Constructor_WithDefaults_UsesDefaultValues()
    {
        // Default should be 10 req/s, burst 20
        TokenBucketRateLimiter.DefaultTokensPerPeriod.Should().Be(10);
        TokenBucketRateLimiter.DefaultTokenLimit.Should().Be(20);
    }

    [Fact]
    public void Constructor_WithInvalidTokensPerSecond_Throws()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new TokenBucketRateLimiter(0, 10));
        Assert.Throws<ArgumentOutOfRangeException>(() => new TokenBucketRateLimiter(-1, 10));
    }

    [Fact]
    public void Constructor_WithInvalidTokenLimit_Throws()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new TokenBucketRateLimiter(10, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new TokenBucketRateLimiter(10, -1));
    }

    [Fact]
    public async Task WaitAsync_FirstRequest_CompletesImmediately()
    {
        // Arrange
        var sw = System.Diagnostics.Stopwatch.StartNew();

        // Act
        await _limiter.WaitAsync();
        sw.Stop();

        // Assert - should complete very quickly (under 50ms)
        sw.ElapsedMilliseconds.Should().BeLessThan(50);
    }

    [Fact]
    public async Task WaitAsync_BurstRequests_CompletesWithinBurstLimit()
    {
        // Arrange
        var sw = System.Diagnostics.Stopwatch.StartNew();

        // Act - make 20 requests (the burst limit)
        for (var i = 0; i < 20; i++)
        {
            await _limiter.WaitAsync();
        }

        sw.Stop();

        // Assert - burst should complete quickly
        sw.ElapsedMilliseconds.Should().BeLessThan(200);
    }

    [Fact]
    public async Task WaitAsync_ExceedsBurst_DelaysSubsequentRequests()
    {
        // Arrange - use a smaller burst to make test faster
        using var limiter = new TokenBucketRateLimiter(10, 5);
        var sw = System.Diagnostics.Stopwatch.StartNew();

        // Act - make 6 requests (exceeds burst of 5)
        for (var i = 0; i < 6; i++)
        {
            await limiter.WaitAsync();
        }

        sw.Stop();

        // Assert - 6th request should wait for token replenishment
        // At 10 tokens/sec, waiting for 1 token takes ~100ms
        sw.ElapsedMilliseconds.Should().BeGreaterOrEqualTo(50);
    }

    [Fact]
    public async Task WaitAsync_AfterDispose_Throws()
    {
        // Arrange
        _limiter.Dispose();

        // Act & Assert
        await Assert.ThrowsAsync<ObjectDisposedException>(() => _limiter.WaitAsync().AsTask());
    }

    [Fact]
    public async Task WaitAsync_WithCancellation_ThrowsOperationCanceledException()
    {
        // Arrange
        using var limiter = new TokenBucketRateLimiter(1, 1);
        using var cts = new CancellationTokenSource();

        // Exhaust the single token
        await limiter.WaitAsync();

        // Cancel immediately
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => limiter.WaitAsync(cts.Token).AsTask());
    }

    [Fact]
    public void IsThrottling_WhenTokensAvailable_ReturnsFalse()
    {
        // Assert
        _limiter.IsThrottling.Should().BeFalse();
    }

    [Fact]
    public async Task IsThrottling_WhenTokensLow_ReturnsTrue()
    {
        // Arrange - use limiter with limited tokens
        using var limiter = new TokenBucketRateLimiter(1, 5);

        // Consume most tokens (leaving < 5)
        for (var i = 0; i < 3; i++)
        {
            await limiter.WaitAsync();
        }

        // Assert - should be throttling when < 5 tokens available
        limiter.IsThrottling.Should().BeTrue();
    }

    [Fact]
    public void Dispose_MultipleTimes_DoesNotThrow()
    {
        // Act & Assert - should not throw
        _limiter.Dispose();
        _limiter.Dispose();
        _limiter.Dispose();
    }

    [Fact]
    public async Task DisposeAsync_CompletesSuccessfully()
    {
        // Act & Assert
        await _limiter.DisposeAsync();

        // Verify disposed
        await Assert.ThrowsAsync<ObjectDisposedException>(() => _limiter.WaitAsync().AsTask());
    }
}
