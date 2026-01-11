using KalshiSharp.Models.Common;

namespace KalshiSharp.Models.Responses;

/// <summary>
/// Response for listing trades.
/// </summary>
public sealed record TradesResponse : PagedResponse<TradeResponse>
{
    /// <summary>
    /// The trades in this page.
    /// </summary>
    public IReadOnlyList<TradeResponse> Trades { get; init; } = [];

    /// <inheritdoc />
    public override IReadOnlyList<TradeResponse> Items => Trades;
}
