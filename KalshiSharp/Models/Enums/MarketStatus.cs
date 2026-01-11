namespace KalshiSharp.Models.Enums;

/// <summary>
/// Represents the current status of a market.
/// </summary>
/// <remarks>
/// Serialized as lowercase strings: "initialized", "active", "closed", "finalized".
/// </remarks>
public enum MarketStatus
{
    /// <summary>
    /// Market has been created but not yet open for trading.
    /// </summary>
    Initialized,

    /// <summary>
    /// Market is actively open for trading.
    /// </summary>
    Active,

    /// <summary>
    /// Market is closed, no trading allowed.
    /// </summary>
    Closed,

    /// <summary>
    /// Market has been finalized and outcome determined.
    /// </summary>
    Finalized
}
