namespace KalshiSharp.Models.Requests;

/// <summary>
/// Request to amend an existing order on the Kalshi exchange.
/// </summary>
/// <remarks>
/// Only specified fields will be updated. All fields are optional.
/// At least one field must be specified.
/// </remarks>
public sealed record AmendOrderRequest
{
    /// <summary>
    /// The new price in cents (1-99).
    /// </summary>
    public int? Price { get; init; }

    /// <summary>
    /// The new quantity (total contracts).
    /// </summary>
    public int? Count { get; init; }
}
