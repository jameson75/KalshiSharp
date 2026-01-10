using System.Text.Json.Serialization;

namespace KalshiSharp.Models.WebSocket;

/// <summary>
/// Base class for all WebSocket messages received from Kalshi.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(OrderBookUpdate), "orderbook_delta")]
[JsonDerivedType(typeof(OrderBookSnapshotMessage), "orderbook_snapshot")]
[JsonDerivedType(typeof(TradeUpdate), "trade")]
[JsonDerivedType(typeof(OrderUpdate), "order")]
[JsonDerivedType(typeof(HeartbeatMessage), "heartbeat")]
[JsonDerivedType(typeof(SubscriptionConfirmation), "subscribed")]
[JsonDerivedType(typeof(UnsubscriptionConfirmation), "unsubscribed")]
[JsonDerivedType(typeof(ErrorMessage), "error")]
public abstract record WebSocketMessage
{
    /// <summary>
    /// Message type identifier.
    /// </summary>
    [JsonPropertyName("type")]
    public abstract string Type { get; }

    /// <summary>
    /// Server-side sequence number for ordering.
    /// </summary>
    [JsonPropertyName("seq")]
    public long? Sequence { get; init; }

    /// <summary>
    /// Server timestamp when this message was generated (Unix milliseconds).
    /// </summary>
    [JsonPropertyName("ts")]
    public long? Timestamp { get; init; }

    /// <summary>
    /// Gets the timestamp as a DateTimeOffset, if available.
    /// </summary>
    [JsonIgnore]
    public DateTimeOffset? TimestampUtc => Timestamp.HasValue
        ? DateTimeOffset.FromUnixTimeMilliseconds(Timestamp.Value)
        : null;
}

/// <summary>
/// Confirmation that a subscription was successful.
/// </summary>
public sealed record SubscriptionConfirmation : WebSocketMessage
{
    /// <inheritdoc/>
    public override string Type => "subscribed";

    /// <summary>
    /// The channel that was subscribed to (e.g., "orderbook_delta", "trade").
    /// </summary>
    [JsonPropertyName("channel")]
    public string? Channel { get; init; }

    /// <summary>
    /// The market ticker(s) subscribed to.
    /// </summary>
    [JsonPropertyName("markets")]
    public IReadOnlyList<string>? Markets { get; init; }
}

/// <summary>
/// Confirmation that an unsubscription was successful.
/// </summary>
public sealed record UnsubscriptionConfirmation : WebSocketMessage
{
    /// <inheritdoc/>
    public override string Type => "unsubscribed";

    /// <summary>
    /// The channel that was unsubscribed from.
    /// </summary>
    [JsonPropertyName("channel")]
    public string? Channel { get; init; }

    /// <summary>
    /// The market ticker(s) unsubscribed from.
    /// </summary>
    [JsonPropertyName("markets")]
    public IReadOnlyList<string>? Markets { get; init; }
}

/// <summary>
/// Error message from the WebSocket server.
/// </summary>
public sealed record ErrorMessage : WebSocketMessage
{
    /// <inheritdoc/>
    public override string Type => "error";

    /// <summary>
    /// Error code.
    /// </summary>
    [JsonPropertyName("code")]
    public string? Code { get; init; }

    /// <summary>
    /// Human-readable error message.
    /// </summary>
    [JsonPropertyName("msg")]
    public string? Message { get; init; }
}
