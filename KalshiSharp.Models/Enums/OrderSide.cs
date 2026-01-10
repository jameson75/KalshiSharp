namespace KalshiSharp.Models.Enums;

/// <summary>
/// Represents the side of an order (buy/sell direction).
/// </summary>
/// <remarks>
/// Serialized as lowercase strings: "yes", "no".
/// </remarks>
public enum OrderSide
{
    /// <summary>
    /// Buy order - purchasing Yes contracts.
    /// </summary>
    Yes,

    /// <summary>
    /// Sell order - purchasing No contracts (or selling Yes contracts).
    /// </summary>
    No
}
