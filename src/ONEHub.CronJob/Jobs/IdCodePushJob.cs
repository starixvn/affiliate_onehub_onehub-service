using Hangfire;
using Microsoft.Extensions.Options;
using ONEHub.CronJob.Configurations;
using ONEHub.Shared.Services.Interfaces;

namespace ONEHub.CronJob.Jobs;

[DisableConcurrentExecution(60)]
[AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Fail, Attempts = 0)]
public class IdCodePushJob(ILoggerService logger, IDateTimeService dateTimeService) : JobBase(logger, dateTimeService)
{
    protected override Task ExecuteAsync()
    {
        // TODO: Implement job logic
        return Task.CompletedTask;
    }

    /// <summary>
    /// Schedules this job with Hangfire
    /// </summary>
    public static void Schedule(IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(services);

        var jobSettings = services.GetRequiredService<IOptions<JobSettings>>().Value;
        ArgumentNullException.ThrowIfNull(jobSettings);

        var config = jobSettings.GetByName(nameof(IdCodePushJob));
        ArgumentNullException.ThrowIfNull(config);

        RecurringJob.AddOrUpdate<IdCodePushJob>(
            nameof(IdCodePushJob),
            job => job.RunAsync(),
            config.CronSchedule,
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Utc
            });
    }
}