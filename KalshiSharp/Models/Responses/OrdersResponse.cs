using KalshiSharp.Models.Common;

namespace KalshiSharp.Models.Responses;

/// <summary>
/// Response for listing orders.
/// </summary>
public sealed record OrdersResponse : PagedResponse<OrderResponse>
{
    /// <summary>
    /// The orders in this page.
    /// </summary>
    public IReadOnlyList<OrderResponse> Orders { get; init; } = [];

    /// <inheritdoc />
    public override IReadOnlyList<OrderResponse> Items => Orders;
}
