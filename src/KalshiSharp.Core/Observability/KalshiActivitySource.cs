using System.Diagnostics;

namespace KalshiSharp.Core.Observability;

/// <summary>
/// OpenTelemetry ActivitySource for KalshiSharp tracing.
/// </summary>
public static class KalshiActivitySource
{
    /// <summary>
    /// The name of the activity source.
    /// </summary>
    public const string Name = "KalshiSharp";

    /// <summary>
    /// The version of the activity source.
    /// </summary>
    public static readonly string Version = typeof(KalshiActivitySource).Assembly.GetName().Version?.ToString() ?? "1.0.0";

    /// <summary>
    /// The shared activity source instance.
    /// </summary>
    public static readonly ActivitySource Source = new(Name, Version);

    /// <summary>
    /// Activity tag names.
    /// </summary>
    public static class Tags
    {
        /// <summary>HTTP method tag.</summary>
        public const string HttpMethod = "http.request.method";

        /// <summary>HTTP status code tag.</summary>
        public const string HttpStatusCode = "http.response.status_code";

        /// <summary>URL path tag.</summary>
        public const string UrlPath = "url.path";

        /// <summary>Request ID tag.</summary>
        public const string RequestId = "kalshi.request_id";

        /// <summary>Market ticker tag.</summary>
        public const string MarketTicker = "kalshi.market";

        /// <summary>Order ID tag.</summary>
        public const string OrderId = "kalshi.order_id";

        /// <summary>Error type tag.</summary>
        public const string ErrorType = "error.type";
    }

    /// <summary>
    /// Span names.
    /// </summary>
    public static class Spans
    {
        /// <summary>HTTP request span name.</summary>
        public const string HttpRequest = "kalshi.http.request";

        /// <summary>WebSocket connect span name.</summary>
        public const string WebSocketConnect = "kalshi.ws.connect";

        /// <summary>WebSocket subscribe span name.</summary>
        public const string WebSocketSubscribe = "kalshi.ws.subscribe";
    }

    /// <summary>
    /// Starts an HTTP request activity.
    /// </summary>
    /// <param name="method">The HTTP method.</param>
    /// <param name="path">The request path.</param>
    /// <returns>The started activity, or null if tracing is not enabled.</returns>
    public static Activity? StartHttpRequest(string method, string path)
    {
        var activity = Source.StartActivity(Spans.HttpRequest, ActivityKind.Client);
        if (activity is not null)
        {
            activity.SetTag(Tags.HttpMethod, method);
            activity.SetTag(Tags.UrlPath, path);
        }
        return activity;
    }
}
