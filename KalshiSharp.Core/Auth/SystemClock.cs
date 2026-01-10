namespace KalshiSharp.Core.Auth;

/// <summary>
/// Default implementation of <see cref="ISystemClock"/> that returns the actual system time.
/// </summary>
public sealed class SystemClock : ISystemClock
{
    /// <inheritdoc />
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
