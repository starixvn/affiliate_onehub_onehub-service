using ONEHub.Shared.Services.Interfaces;

namespace ONEHub.CronJob.Jobs;

/// <summary>
/// Job for pulling data from external API
/// </summary>
public class CustomerPullJob(ILoggerService logger, IDateTimeService dateTimeService) : JobBase(logger, dateTimeService)
{
    protected override Task ExecuteAsync()
    {
        return Task.CompletedTask;
    }
}