namespace KalshiSharp.Models.Responses;

/// <summary>
/// Wrapper response for the order book endpoint.
/// </summary>
public sealed record OrderBookResponse
{
    /// <summary>
    /// The order book data.
    /// </summary>
    public required OrderBookData Orderbook { get; init; }
}

/// <summary>
/// Represents the order book data for a market.
/// </summary>
public sealed record OrderBookData
{
    private readonly IReadOnlyList<int[]>? _yes;
    private readonly IReadOnlyList<int[]>? _no;

    /// <summary>
    /// Bids on the Yes side, ordered by price descending.
    /// Each entry is [price, quantity].
    /// </summary>
    public IReadOnlyList<int[]> Yes
    {
        get => _yes ?? [];
        init => _yes = value;
    }

    /// <summary>
    /// Bids on the No side, ordered by price descending.
    /// Each entry is [price, quantity].
    /// </summary>
    public IReadOnlyList<int[]> No
    {
        get => _no ?? [];
        init => _no = value;
    }
}
