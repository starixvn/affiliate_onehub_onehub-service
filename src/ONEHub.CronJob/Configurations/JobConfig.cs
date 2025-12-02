using Hangfire;
using Microsoft.Extensions.Options;
using ONEHub.CronJob.Jobs;
using ONEHub.CronJob.Jobs.OHCustomer;
using ONEHub.Shared.Services.Interfaces;

namespace ONEHub.CronJob.Configurations;

/// <summary>
/// Configuration for all cronjob settings (binds directly to array in appsettings.json)
/// </summary>
public class JobSettings : List<JobConfiguration>
{
    private Dictionary<string, JobConfiguration>? _lookup;

    /// <summary>
    /// Gets job configuration by name
    /// </summary>
    public JobConfiguration GetByName(string name)
    {
        _lookup ??= this.ToDictionary(j => j.Name, j => j);
        return _lookup.GetValueOrDefault(name)!;
    }
}

/// <summary>
/// Configuration for a job
/// </summary>
public class JobConfiguration
{
    public string Name { get; set; } = null!;

    public string CronSchedule { get; set; } = null!;

    public int MaxBatchSize { get; set; } = 100;

    /// <summary>
    /// Maximum time range in minutes for data pull
    /// </summary>
    public int MaxTimeRange { get; set; } = 30;

    /// <summary>
    /// Data source identifier for tracking (e.g., "MemberInfoLastImportedTime")
    /// </summary>
    public string DataSource { get; set; } = string.Empty;
}

/// <summary>
/// Job configuration extensions
/// </summary>
public static class JobConfig
{
    /// <summary>
    /// Schedules all recurring jobs and registers job trigger endpoints
    /// </summary>
    public static WebApplication UseCronJobs(this WebApplication app)
    {
        var jobSettings = app.Services.GetRequiredService<IOptions<JobSettings>>().Value;

        // Schedule customer pull jobs
        ScheduleJob<OHCustomerPullJob>(jobSettings);
        ScheduleJob<OHCustomerRealTimePullJob>(jobSettings);
        ScheduleJob<OHCustomerManualPullJob>(jobSettings);

        // Schedule other jobs
        ScheduleJob<IdCodePushJob>(jobSettings);

        // Register trigger endpoints for each job
        MapJobTriggerEndpoint<OHCustomerPullJob>(app);
        MapJobTriggerEndpoint<OHCustomerRealTimePullJob>(app);
        MapJobTriggerEndpoint<OHCustomerManualPullJob>(app);
        MapJobTriggerEndpoint<IdCodePushJob>(app);

        return app;
    }

    /// <summary>
    /// Schedules a single job with Hangfire using job configuration
    /// </summary>
    private static void ScheduleJob<T>(JobSettings jobSettings) where T : JobBase
    {
        var jobName = typeof(T).Name;
        var config = jobSettings.GetByName(jobName);
        ArgumentNullException.ThrowIfNull(config);

        RecurringJob.AddOrUpdate<T>(
            jobName,
            job => job.RunAsync(),
            config.CronSchedule,
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.Utc
            });
    }

    /// <summary>
    /// Maps trigger endpoint for a specific job: POST /api/jobs/trigger/{jobName}
    /// </summary>
    private static void MapJobTriggerEndpoint<T>(IEndpointRouteBuilder app) where T : JobBase
    {
        var jobName = typeof(T).Name;

        app.MapPost($"/api/jobs/trigger/{jobName}", (IBackgroundJobClient backgroundJobClient, IServiceProvider serviceProvider) =>
        {
            var jobId = backgroundJobClient.Enqueue<T>(job => job.RunAsync());
            var dateTimeService = serviceProvider.GetRequiredService<IDateTimeService>();

            return Results.Ok(new
            {
                JobId = jobId,
                JobName = jobName,
                Message = "Job queued for execution",
                TriggeredAt = dateTimeService.UtcNow
            });
        })
        .WithTags("Job Triggers")
        .WithName($"Trigger{jobName}")
        .WithSummary($"Manually trigger {jobName}")
        .WithDescription($"Queues {jobName} for immediate execution in Hangfire")
        .Produces<object>();
    }
}
