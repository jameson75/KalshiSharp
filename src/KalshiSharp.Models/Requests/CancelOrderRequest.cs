namespace KalshiSharp.Models.Requests;

/// <summary>
/// Request to cancel an existing order on the Kalshi exchange.
/// </summary>
/// <remarks>
/// This is a marker type for the cancel operation. The order ID is
/// passed as a path parameter to the DELETE endpoint.
/// </remarks>
public sealed record CancelOrderRequest
{
    // Intentionally empty - order ID is passed as path parameter
}
