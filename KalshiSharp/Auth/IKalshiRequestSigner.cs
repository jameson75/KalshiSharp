namespace KalshiSharp.Auth;

/// <summary>
/// Signs HTTP requests for Kalshi API authentication.
/// </summary>
public interface IKalshiRequestSigner
{
    /// <summary>
    /// Signs an HTTP request by adding authentication headers.
    /// </summary>
    /// <param name="request">The HTTP request to sign.</param>
    /// <param name="body">The request body bytes (empty span for bodyless requests).</param>
    /// <param name="timestamp">The timestamp to use for signing.</param>
    /// <remarks>
    /// This method adds the following headers to the request:
    /// <list type="bullet">
    ///   <item><c>KALSHI-ACCESS-KEY</c>: The API key</item>
    ///   <item><c>KALSHI-ACCESS-TIMESTAMP</c>: Unix timestamp in milliseconds</item>
    ///   <item><c>KALSHI-ACCESS-SIGNATURE</c>: Base64-encoded HMAC-SHA256 signature</item>
    /// </list>
    /// </remarks>
    void Sign(HttpRequestMessage request, ReadOnlySpan<byte> body, DateTimeOffset timestamp);
}
