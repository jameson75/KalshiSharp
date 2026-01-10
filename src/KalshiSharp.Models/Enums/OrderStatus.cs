namespace KalshiSharp.Models.Enums;

/// <summary>
/// Represents the current status of an order.
/// </summary>
/// <remarks>
/// Serialized as lowercase strings: "resting", "canceled", "executed", "pending".
/// </remarks>
public enum OrderStatus
{
    /// <summary>
    /// Order is active and resting on the order book.
    /// </summary>
    Resting,

    /// <summary>
    /// Order has been cancelled.
    /// </summary>
    Canceled,

    /// <summary>
    /// Order has been fully executed.
    /// </summary>
    Executed,

    /// <summary>
    /// Order is pending execution or placement.
    /// </summary>
    Pending
}
