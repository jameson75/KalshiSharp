using System.Text.Json.Serialization;

namespace KalshiSharp.Models.WebSocket;

/// <summary>
/// Heartbeat message sent periodically by the server to keep the connection alive.
/// </summary>
public sealed record HeartbeatMessage : WebSocketMessage
{
    /// <inheritdoc/>
    public override string Type => "heartbeat";

    /// <summary>
    /// Server-side identifier for this heartbeat.
    /// </summary>
    [JsonPropertyName("id")]
    public long? Id { get; init; }
}
