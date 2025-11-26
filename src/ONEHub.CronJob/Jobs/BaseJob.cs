using ONEHub.Shared.Services.Interfaces;

namespace ONEHub.CronJob.Jobs;

public abstract class JobBase(ILoggerService logger, IDateTimeService dateTimeService)
{
    public async Task RunAsync()
    {
        try
        {
            logger.LogInfo($"Job {GetType().Name} started at {dateTimeService.UtcNow}");
            await ExecuteAsync();
            logger.LogInfo($"Job {GetType().Name} completed at {dateTimeService.UtcNow}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex);
            throw;
        }
    }

    protected abstract Task ExecuteAsync();
}