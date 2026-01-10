namespace KalshiSharp.Core.Configuration;

/// <summary>
/// Configuration options for the Kalshi API client.
/// </summary>
public sealed class KalshiClientOptions
{
    private static readonly Uri ProductionUri = new("https://trading-api.kalshi.com");
    private static readonly Uri DemoUri = new("https://demo-api.kalshi.com");

    /// <summary>
    /// The API key for authentication.
    /// </summary>
    public required string ApiKey { get; init; }

    /// <summary>
    /// The API secret for request signing.
    /// </summary>
    public required string ApiSecret { get; init; }

    /// <summary>
    /// The target environment. Defaults to <see cref="KalshiEnvironment.Production"/>.
    /// </summary>
    public KalshiEnvironment Environment { get; init; } = KalshiEnvironment.Production;

    /// <summary>
    /// Optional base URI override. If not specified, derived from <see cref="Environment"/>.
    /// </summary>
    public Uri? BaseUri { get; init; }

    /// <summary>
    /// HTTP request timeout. Defaults to 30 seconds.
    /// </summary>
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Tolerance for clock skew between client and server. Defaults to 30 seconds.
    /// </summary>
    public TimeSpan ClockSkewTolerance { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Whether to enable client-side rate limiting. Defaults to true.
    /// </summary>
    public bool EnableRateLimiting { get; init; } = true;

    /// <summary>
    /// Gets the effective base URI, using <see cref="BaseUri"/> if specified,
    /// otherwise deriving from <see cref="Environment"/>.
    /// </summary>
    /// <returns>The base URI to use for API requests.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <see cref="Environment"/> is invalid.</exception>
    public Uri GetEffectiveBaseUri() => BaseUri ?? Environment switch
    {
        KalshiEnvironment.Production => ProductionUri,
        KalshiEnvironment.Demo => DemoUri,
        _ => throw new ArgumentOutOfRangeException(nameof(Environment), Environment, "Invalid environment value.")
    };
}
