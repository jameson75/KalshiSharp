namespace KalshiSharp.Core.Auth;

/// <summary>
/// Abstraction for system time to enable testability of time-dependent operations.
/// </summary>
public interface ISystemClock
{
    /// <summary>
    /// Gets the current UTC time.
    /// </summary>
    DateTimeOffset UtcNow { get; }
}
