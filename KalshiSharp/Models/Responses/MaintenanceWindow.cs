namespace KalshiSharp.Models.Responses;

/// <summary>
/// Represents a maintenance window when trading is unavailable.
/// </summary>
public sealed record MaintenanceWindow
{
    /// <summary>
    /// When the maintenance window starts.
    /// </summary>
    public DateTimeOffset? StartDatetime { get; init; }

    /// <summary>
    /// When the maintenance window ends.
    /// </summary>
    public DateTimeOffset? EndDatetime { get; init; }
}
