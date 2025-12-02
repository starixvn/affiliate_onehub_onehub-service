namespace ONEHub.CronJob.Helpers;

/// <summary>
/// Helper class for date/time operations
/// </summary>
public static class DateTimeHelper
{
    /// <summary>
    /// Splits a large time range into smaller proportional ranges
    /// This prevents timeouts and allows progress tracking per range
    /// </summary>
    /// <param name="startTime">Start of the total time range</param>
    /// <param name="endTime">End of the total time range</param>
    /// <param name="span">Maximum duration for each sub-range</param>
    /// <param name="isExcludeNotFullRangeSpan">If true, excludes the last range if it's not a full span</param>
    /// <returns>Enumerable of (FromDate, ToDate) tuples</returns>
    /// <example>
    /// GetProportionTimeRange(10:00, 10:35, 15 minutes) 
    /// Returns: [(10:00, 10:15), (10:15, 10:30), (10:30, 10:35)]
    /// </example>
    public static IEnumerable<(DateTime FromDate, DateTime ToDate)> GetProportionTimeRange(
        DateTime startTime,
        DateTime endTime,
        TimeSpan span,
        bool isExcludeNotFullRangeSpan = true)
    {
        if (span == TimeSpan.Zero)
        {
            throw new ArgumentException("Zero time span, this would create an infinite loop.", nameof(span));
        }

        if (startTime >= endTime)
        {
            yield break; // No ranges to return
        }

        var currentStart = startTime;
        var currentEnd = GetSmallerTime(currentStart.Add(span), endTime);

        while (currentStart < currentEnd)
        {
            // Only yield if:
            // 1. We're including partial ranges, OR
            // 2. This is a full span range
            if (!isExcludeNotFullRangeSpan || currentEnd - currentStart == span)
            {
                yield return (currentStart, currentEnd);
            }

            currentStart = currentEnd;
            currentEnd = GetSmallerTime(currentEnd.Add(span), endTime);
        }
    }

    /// <summary>
    /// Returns the smaller of two DateTime values
    /// </summary>
    public static DateTime GetSmallerTime(DateTime compareTime, DateTime endTime)
        => DateTime.Compare(compareTime, endTime) > 0 ? endTime : compareTime;
}

