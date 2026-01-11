using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using KalshiSharp.Configuration;
using Microsoft.Extensions.Options;

namespace KalshiSharp.Auth;

/// <summary>
/// Signs Kalshi API requests using RSA-PSS with SHA256.
/// </summary>
/// <remarks>
/// Kalshi API requires RSA-PSS signatures with the following format:
/// <code>
/// message = timestamp_ms + http_method + path_without_query
/// signature = RSA-PSS-Sign(message, SHA256, salt_length=digest_length)
/// </code>
/// </remarks>
public sealed class RsaPssRequestSigner : IKalshiRequestSigner, IDisposable
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

    private readonly string _apiKeyId;
    private readonly RSA _rsa;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="RsaPssRequestSigner"/> class.
    /// </summary>
    /// <param name="options">The client options containing API credentials.</param>
    public RsaPssRequestSigner(IOptions<KalshiClientOptions> options)
        : this(options.Value.ApiKey, options.Value.ApiSecret)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RsaPssRequestSigner"/> class.
    /// </summary>
    /// <param name="apiKeyId">The API key ID.</param>
    /// <param name="privateKeyPem">The RSA private key in PEM format.</param>
    public RsaPssRequestSigner(string apiKeyId, string privateKeyPem)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKeyId);
        ArgumentException.ThrowIfNullOrWhiteSpace(privateKeyPem);

        _apiKeyId = apiKeyId;
        _rsa = RSA.Create();

        try
        {
            _rsa.ImportFromPem(privateKeyPem.AsSpan());
        }
        catch (Exception ex)
        {
            _rsa.Dispose();
            throw new ArgumentException(
                "Failed to import RSA private key. Ensure the key is in valid PEM format (RSA PRIVATE KEY).",
                nameof(privateKeyPem),
                ex);
        }
    }

    /// <inheritdoc />
    public void Sign(HttpRequestMessage request, ReadOnlySpan<byte> body, DateTimeOffset timestamp)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(request);

        var timestampMs = timestamp.ToUnixTimeMilliseconds();
        var timestampStr = timestampMs.ToString(CultureInfo.InvariantCulture);

        // Get path WITHOUT query parameters (required by Kalshi API)
        var fullPath = request.RequestUri?.PathAndQuery ?? "/";
        var pathWithoutQuery = GetPathWithoutQuery(fullPath);

        // Build message: timestamp + method + path (no separators, no body)
        var method = request.Method.Method.ToUpperInvariant();
        var message = timestampStr + method + pathWithoutQuery;

        var signature = SignMessage(message);

        request.Headers.Remove(AccessKeyHeader);
        request.Headers.Remove(AccessTimestampHeader);
        request.Headers.Remove(AccessSignatureHeader);

        request.Headers.TryAddWithoutValidation(AccessKeyHeader, _apiKeyId);
        request.Headers.TryAddWithoutValidation(AccessTimestampHeader, timestampStr);
        request.Headers.TryAddWithoutValidation(AccessSignatureHeader, signature);
    }

    /// <summary>
    /// Signs a message using RSA-PSS with SHA256.
    /// </summary>
    /// <param name="message">The message to sign.</param>
    /// <returns>Base64-encoded signature.</returns>
    internal string SignMessage(string message)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var messageBytes = Encoding.UTF8.GetBytes(message);

        var signatureBytes = _rsa.SignData(
            messageBytes,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pss);

        return Convert.ToBase64String(signatureBytes);
    }

    /// <summary>
    /// Extracts the path without query parameters.
    /// </summary>
    /// <param name="pathAndQuery">The full path including query string.</param>
    /// <returns>The path without query parameters.</returns>
    private static string GetPathWithoutQuery(string pathAndQuery)
    {
        var queryIndex = pathAndQuery.IndexOf('?');
        return queryIndex >= 0 ? pathAndQuery[..queryIndex] : pathAndQuery;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _rsa.Dispose();
        _disposed = true;
    }
}
