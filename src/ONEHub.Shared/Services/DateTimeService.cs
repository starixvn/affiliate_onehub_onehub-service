using ONEHub.Shared.Services.Interfaces;

namespace ONEHub.Shared.Services;

/// <summary>
/// Service for DateTime operations using DateTimeOffset for better timezone handling
/// </summary>
public class DateTimeService : IDateTimeService
{
    /// <inheritdoc />
    public DateTime UtcNow => DateTimeOffset.UtcNow.UtcDateTime;

    /// <inheritdoc />
    public DateTime MinValue => DateTimeOffset.MinValue.DateTime;
}
