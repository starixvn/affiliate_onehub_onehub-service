namespace ONEHub.Shared.Services.Interfaces;

/// <summary>
/// Service for DateTime operations to improve testability
/// </summary>
public interface IDateTimeService
{
    /// <summary>
    /// Gets the current UTC date and time
    /// </summary>
    DateTime UtcNow { get; }

    /// <summary>
    /// Gets the minimum DateTime value
    /// </summary>
    DateTime MinValue { get; }
}
