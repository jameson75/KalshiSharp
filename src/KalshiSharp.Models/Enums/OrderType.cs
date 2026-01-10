namespace KalshiSharp.Models.Enums;

/// <summary>
/// Represents the type of order execution.
/// </summary>
/// <remarks>
/// Serialized as lowercase strings: "limit", "market".
/// </remarks>
public enum OrderType
{
    /// <summary>
    /// Limit order - executes at the specified price or better.
    /// </summary>
    Limit,

    /// <summary>
    /// Market order - executes immediately at the best available price.
    /// </summary>
    Market
}
