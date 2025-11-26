using ONEHub.CronJob.Jobs;

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

    public string AdjustmentValue { get; set; } = null!;

    public string CronSchedule { get; set; } = null!;

    public string MaxTimeRange { get; set; } = null!;

    public string DataSource { get; set; } = null!;
}

/// <summary>
/// Job configuration extensions
/// </summary>
public static class JobConfig
{
    /// <summary>
    /// Schedules all recurring jobs
    /// </summary>
    public static WebApplication UseCronJobs(this WebApplication app)
    {
        CustomerPullJob.Schedule(app.Services);
        IdCodePushJob.Schedule(app.Services);
        return app;
    }
}
