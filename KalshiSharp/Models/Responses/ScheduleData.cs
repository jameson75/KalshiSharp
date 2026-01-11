namespace KalshiSharp.Models.Responses;

/// <summary>
/// Contains the schedule data with standard trading hours and maintenance windows.
/// </summary>
public sealed record ScheduleData
{
    /// <summary>
    /// Standard trading hours by day of week.
    /// </summary>
    public IReadOnlyList<StandardHoursEntry> StandardHours { get; init; } = [];

    /// <summary>
    /// Scheduled maintenance windows when trading is unavailable.
    /// </summary>
    public IReadOnlyList<MaintenanceWindow> MaintenanceWindows { get; init; } = [];
}
