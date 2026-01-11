namespace KalshiSharp.Models.Responses;

/// <summary>
/// Represents trading hours with open and close times.
/// </summary>
public sealed record TradingHours
{
    /// <summary>
    /// Market open time in ET (e.g., "09:30").
    /// </summary>
    public string? OpenTime { get; init; }

    /// <summary>
    /// Market close time in ET (e.g., "16:00").
    /// </summary>
    public string? CloseTime { get; init; }
}
