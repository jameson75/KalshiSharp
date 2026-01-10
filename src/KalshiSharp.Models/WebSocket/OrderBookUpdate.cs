using System.Text.Json.Serialization;

namespace KalshiSharp.Models.WebSocket;

/// <summary>
/// Delta update for an order book. Contains only the changes since the last update.
/// </summary>
public sealed record OrderBookUpdate : WebSocketMessage
{
    /// <inheritdoc/>
    public override string Type => "orderbook_delta";

    /// <summary>
    /// Market ticker this update is for.
    /// </summary>
    [JsonPropertyName("market_ticker")]
    public required string MarketTicker { get; init; }

    /// <summary>
    /// Price level for this update (1-99 cents).
    /// </summary>
    [JsonPropertyName("price")]
    public required int Price { get; init; }

    /// <summary>
    /// Change in quantity at this price level for Yes side.
    /// Positive = added, negative = removed.
    /// </summary>
    [JsonPropertyName("delta")]
    public required int Delta { get; init; }

    /// <summary>
    /// Side of the order book: "yes" or "no".
    /// </summary>
    [JsonPropertyName("side")]
    public required string Side { get; init; }

    /// <summary>
    /// Whether this is the Yes side.
    /// </summary>
    [JsonIgnore]
    public bool IsYesSide => string.Equals(Side, "yes", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Whether this is the No side.
    /// </summary>
    [JsonIgnore]
    public bool IsNoSide => string.Equals(Side, "no", StringComparison.OrdinalIgnoreCase);
}

/// <summary>
/// Full snapshot of an order book. Sent on initial subscription.
/// </summary>
public sealed record OrderBookSnapshotMessage : WebSocketMessage
{
    /// <inheritdoc/>
    public override string Type => "orderbook_snapshot";

    /// <summary>
    /// Market ticker this snapshot is for.
    /// </summary>
    [JsonPropertyName("market_ticker")]
    public required string MarketTicker { get; init; }

    /// <summary>
    /// Bids on the Yes side. Each entry is [price, quantity].
    /// </summary>
    [JsonPropertyName("yes")]
    public required IReadOnlyList<int[]> Yes { get; init; }

    /// <summary>
    /// Bids on the No side. Each entry is [price, quantity].
    /// </summary>
    [JsonPropertyName("no")]
    public required IReadOnlyList<int[]> No { get; init; }
}
