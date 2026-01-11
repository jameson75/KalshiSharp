namespace KalshiSharp.Models.Responses;

/// <summary>
/// Represents a standard hours entry with daily trading schedules.
/// </summary>
public sealed record StandardHoursEntry
{
    /// <summary>
    /// When this schedule becomes effective.
    /// </summary>
    public DateTimeOffset? StartTime { get; init; }

    /// <summary>
    /// When this schedule ends.
    /// </summary>
    public DateTimeOffset? EndTime { get; init; }

    /// <summary>
    /// Trading hours for Monday.
    /// </summary>
    public IReadOnlyList<TradingHours> Monday { get; init; } = [];

    /// <summary>
    /// Trading hours for Tuesday.
    /// </summary>
    public IReadOnlyList<TradingHours> Tuesday { get; init; } = [];

    /// <summary>
    /// Trading hours for Wednesday.
    /// </summary>
    public IReadOnlyList<TradingHours> Wednesday { get; init; } = [];

    /// <summary>
    /// Trading hours for Thursday.
    /// </summary>
    public IReadOnlyList<TradingHours> Thursday { get; init; } = [];

    /// <summary>
    /// Trading hours for Friday.
    /// </summary>
    public IReadOnlyList<TradingHours> Friday { get; init; } = [];

    /// <summary>
    /// Trading hours for Saturday.
    /// </summary>
    public IReadOnlyList<TradingHours> Saturday { get; init; } = [];

    /// <summary>
    /// Trading hours for Sunday.
    /// </summary>
    public IReadOnlyList<TradingHours> Sunday { get; init; } = [];
}
