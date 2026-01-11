using System.Security.Cryptography;
using System.Text;
using KalshiSharp.Configuration;
using Microsoft.Extensions.Options;

namespace KalshiSharp.Auth;

/// <summary>
/// Signs Kalshi API requests using HMAC-SHA256.
/// </summary>
public sealed class HmacSha256RequestSigner : IKalshiRequestSigner, IDisposable
{
    /// <summary>
    /// Header name for the API key.
    /// </summary>
    public const string AccessKeyHeader = "KALSHI-ACCESS-KEY";

    /// <summary>
    /// Header name for the timestamp.
    /// </summary>
    public const string AccessTimestampHeader = "KALSHI-ACCESS-TIMESTAMP";

    /// <summary>
    /// Header name for the signature.
    /// </summary>
    public const string AccessSignatureHeader = "KALSHI-ACCESS-SIGNATURE";

    private readonly string _apiKey;
    private readonly HMACSHA256 _hmac;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="HmacSha256RequestSigner"/> class.
    /// </summary>
    /// <param name="options">The client options containing API credentials.</param>
    public HmacSha256RequestSigner(IOptions<KalshiClientOptions> options)
        : this(options.Value.ApiKey, options.Value.ApiSecret)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HmacSha256RequestSigner"/> class.
    /// </summary>
    /// <param name="apiKey">The API key.</param>
    /// <param name="apiSecret">The API secret for signing.</param>
    public HmacSha256RequestSigner(string apiKey, string apiSecret)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(apiSecret);

        _apiKey = apiKey;
        _hmac = new HMACSHA256(Encoding.UTF8.GetBytes(apiSecret));
    }

    /// <inheritdoc />
    public void Sign(HttpRequestMessage request, ReadOnlySpan<byte> body, DateTimeOffset timestamp)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(request);

        var timestampMs = timestamp.ToUnixTimeMilliseconds();
        var canonicalRequest = CanonicalRequestBuilder.Build(timestampMs, request, body);
        var signature = ComputeSignature(canonicalRequest);

        request.Headers.Remove(AccessKeyHeader);
        request.Headers.Remove(AccessTimestampHeader);
        request.Headers.Remove(AccessSignatureHeader);

        request.Headers.TryAddWithoutValidation(AccessKeyHeader, _apiKey);
        request.Headers.TryAddWithoutValidation(AccessTimestampHeader, timestampMs.ToString(System.Globalization.CultureInfo.InvariantCulture));
        request.Headers.TryAddWithoutValidation(AccessSignatureHeader, signature);
    }

    /// <summary>
    /// Computes the HMAC-SHA256 signature for the given data.
    /// </summary>
    /// <param name="data">The data to sign.</param>
    /// <returns>Base64-encoded signature.</returns>
    internal string ComputeSignature(byte[] data)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var hash = _hmac.ComputeHash(data);
        return Convert.ToBase64String(hash);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _hmac.Dispose();
        _disposed = true;
    }
}
