using System.Buffers;
using System.Globalization;
using System.Text;

namespace KalshiSharp.Auth;

/// <summary>
/// Builds canonical request strings for Kalshi API request signing.
/// </summary>
/// <remarks>
/// The canonical request format is:
/// <code>
/// {timestamp}\n{method}\n{path}\n{body}
/// </code>
/// Where:
/// <list type="bullet">
///   <item><c>timestamp</c>: Unix timestamp in milliseconds</item>
///   <item><c>method</c>: HTTP method (uppercase)</item>
///   <item><c>path</c>: Request path with query string</item>
///   <item><c>body</c>: Request body (empty string if no body)</item>
/// </list>
/// </remarks>
public static class CanonicalRequestBuilder
{
    private const byte NewlineByte = (byte)'\n';

    /// <summary>
    /// Builds a canonical request string as UTF-8 bytes.
    /// </summary>
    /// <param name="timestampMs">Unix timestamp in milliseconds.</param>
    /// <param name="method">HTTP method (will be uppercased).</param>
    /// <param name="pathAndQuery">Request path including query string.</param>
    /// <param name="body">Request body bytes.</param>
    /// <returns>UTF-8 encoded canonical request bytes.</returns>
    public static byte[] Build(long timestampMs, string method, string pathAndQuery, ReadOnlySpan<byte> body)
    {
        // Format: {timestamp}\n{method}\n{path}\n{body}
        var timestampStr = timestampMs.ToString(CultureInfo.InvariantCulture);
        var upperMethod = method.ToUpperInvariant();

        // Calculate required size
        var timestampByteCount = Encoding.UTF8.GetByteCount(timestampStr);
        var methodByteCount = Encoding.UTF8.GetByteCount(upperMethod);
        var pathByteCount = Encoding.UTF8.GetByteCount(pathAndQuery);
        var totalSize = timestampByteCount + 1 + methodByteCount + 1 + pathByteCount + 1 + body.Length;

        var result = new byte[totalSize];
        var offset = 0;

        // Write timestamp
        offset += Encoding.UTF8.GetBytes(timestampStr, result.AsSpan(offset));
        result[offset++] = NewlineByte;

        // Write method
        offset += Encoding.UTF8.GetBytes(upperMethod, result.AsSpan(offset));
        result[offset++] = NewlineByte;

        // Write path
        offset += Encoding.UTF8.GetBytes(pathAndQuery, result.AsSpan(offset));
        result[offset++] = NewlineByte;

        // Write body
        body.CopyTo(result.AsSpan(offset));

        return result;
    }

    /// <summary>
    /// Builds a canonical request string as UTF-8 bytes using an HTTP request message.
    /// </summary>
    /// <param name="timestampMs">Unix timestamp in milliseconds.</param>
    /// <param name="request">The HTTP request message.</param>
    /// <param name="body">Request body bytes.</param>
    /// <returns>UTF-8 encoded canonical request bytes.</returns>
    public static byte[] Build(long timestampMs, HttpRequestMessage request, ReadOnlySpan<byte> body)
    {
        var method = request.Method.Method;
        var pathAndQuery = request.RequestUri?.PathAndQuery ?? "/";

        return Build(timestampMs, method, pathAndQuery, body);
    }
}
