using Hangfire;
using Microsoft.Extensions.Options;
using ONEHub.CronJob.Configurations;
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

    /// <summary>
    /// Schedules this job with Hangfire
    /// </summary>
    public static void Schedule(IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(services);

        var jobSettings = services.GetRequiredService<IOptions<JobSettings>>().Value;
        ArgumentNullException.ThrowIfNull(jobSettings);

        var config = jobSettings.GetByName(nameof(CustomerPullJob));
        ArgumentNullException.ThrowIfNull(config);

        RecurringJob.AddOrUpdate<CustomerPullJob>(
            nameof(CustomerPullJob),
            job => job.RunAsync(),
            config.CronSchedule,
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Utc
            });
    }
}