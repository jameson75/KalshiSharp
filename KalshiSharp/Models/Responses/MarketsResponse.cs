using KalshiSharp.Models.Common;

namespace KalshiSharp.Models.Responses;

/// <summary>
/// Response for listing markets.
/// </summary>
public sealed record MarketsResponse : PagedResponse<MarketResponse>
{
    /// <summary>
    /// The markets in this page.
    /// </summary>
    public IReadOnlyList<MarketResponse> Markets { get; init; } = [];

    /// <inheritdoc />
    public override IReadOnlyList<MarketResponse> Items => Markets;
}
