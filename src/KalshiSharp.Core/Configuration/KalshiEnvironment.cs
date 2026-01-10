namespace KalshiSharp.Core.Configuration;

/// <summary>
/// Specifies the Kalshi API environment to connect to.
/// </summary>
public enum KalshiEnvironment
{
    /// <summary>
    /// Production environment (https://trading-api.kalshi.com).
    /// </summary>
    Production,

    /// <summary>
    /// Demo/sandbox environment (https://demo-api.kalshi.com).
    /// </summary>
    Demo
}
