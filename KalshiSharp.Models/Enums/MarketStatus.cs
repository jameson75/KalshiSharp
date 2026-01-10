namespace KalshiSharp.Models.Enums;

/// <summary>
/// Represents the current status of a market.
/// </summary>
/// <remarks>
/// Serialized as lowercase strings: "open", "closed", "settled".
/// </remarks>
public enum MarketStatus
{
    /// <summary>
    /// Market is open for trading.
    /// </summary>
    Open,

    /// <summary>
    /// Market is closed, no trading allowed.
    /// </summary>
    Closed,

    /// <summary>
    /// Market has settled and outcome determined.
    /// </summary>
    Settled
}
