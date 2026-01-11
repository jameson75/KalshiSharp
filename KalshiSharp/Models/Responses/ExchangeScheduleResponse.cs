namespace KalshiSharp.Models.Responses;

/// <summary>
/// Represents the trading schedule of the Kalshi exchange.
/// </summary>
public sealed record ExchangeScheduleResponse
{
    /// <summary>
    /// The schedule containing standard hours and maintenance windows.
    /// </summary>
    public required ScheduleData Schedule { get; init; }
}
