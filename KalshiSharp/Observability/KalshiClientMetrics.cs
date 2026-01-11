using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace KalshiSharp.Observability;

/// <summary>
/// Metrics for KalshiSharp client operations.
/// </summary>
public sealed class KalshiClientMetrics : IDisposable
{
    private readonly Meter _meter;
    private readonly Counter<long> _requestCounter;
    private readonly Histogram<double> _requestDuration;
    private readonly Counter<long> _errorCounter;
    private readonly Counter<long> _rateLimitCounter;
    private bool _disposed;

    /// <summary>
    /// The meter name for KalshiSharp metrics.
    /// </summary>
    public const string MeterName = "KalshiSharp";

    /// <summary>
    /// Initializes a new instance of the <see cref="KalshiClientMetrics"/> class.
    /// </summary>
    public KalshiClientMetrics()
    {
        var version = typeof(KalshiClientMetrics).Assembly.GetName().Version?.ToString() ?? "1.0.0";
        _meter = new Meter(MeterName, version);

        _requestCounter = _meter.CreateCounter<long>(
            "kalshi.http.requests",
            description: "Total number of HTTP requests made to Kalshi API");

        _requestDuration = _meter.CreateHistogram<double>(
            "kalshi.http.request.duration",
            unit: "ms",
            description: "Duration of HTTP requests to Kalshi API in milliseconds");

        _errorCounter = _meter.CreateCounter<long>(
            "kalshi.http.errors",
            description: "Total number of HTTP errors from Kalshi API");

        _rateLimitCounter = _meter.CreateCounter<long>(
            "kalshi.http.rate_limits",
            description: "Total number of rate limit responses (429) from Kalshi API");
    }

    /// <summary>
    /// Records a completed HTTP request.
    /// </summary>
    /// <param name="method">The HTTP method.</param>
    /// <param name="path">The request path.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="duration">The request duration.</param>
    public void RecordRequest(string method, string path, int statusCode, TimeSpan duration)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var tags = new TagList
        {
            { "http.request.method", method },
            { "url.path", NormalizePath(path) },
            { "http.response.status_code", statusCode }
        };

        _requestCounter.Add(1, tags);
        _requestDuration.Record(duration.TotalMilliseconds, tags);

        if (statusCode >= 400)
        {
            _errorCounter.Add(1, tags);
        }

        if (statusCode == 429)
        {
            _rateLimitCounter.Add(1, tags);
        }
    }

    /// <summary>
    /// Normalizes a path by replacing dynamic segments with placeholders.
    /// </summary>
    private static string NormalizePath(string path)
    {
        // Replace UUIDs and other IDs with placeholders to avoid high cardinality
        // Pattern: /markets/{ticker} -> /markets/{ticker}
        // For now, return as-is; can be enhanced based on actual patterns
        return path;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _meter.Dispose();
        _disposed = true;
    }
}
