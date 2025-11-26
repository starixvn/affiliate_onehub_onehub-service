using Fanex.Data.Repository;
using ONEHub.CronJob.Configurations;
using ONEHub.CronJob.Jobs;
using ONEHub.CronJob.Repositories;
using ONEHub.CronJob.Repositories.Interfaces;
using ONEHub.Shared.Services;
using ONEHub.Shared.Services.Interfaces;

namespace ONEHub.CronJob;

public static class DependencyInjection
{
    public static IServiceCollection AddDependencyInjection(this IServiceCollection services, IConfiguration configuration)
    {
        // Register configurations
        services.Configure<JobSettings>(configuration.GetSection(nameof(JobSettings)));
        services.AddOptions<JobSettings>().ValidateOnStart();

        services.Configure<RedisSettings>(configuration.GetSection(nameof(RedisSettings)));
        services.AddOptions<RedisSettings>().ValidateOnStart();

        services.Configure<HangfireSettings>(configuration.GetSection(nameof(HangfireSettings)));
        services.AddOptions<HangfireSettings>().ValidateOnStart();

        // Get RedisSettings from configuration
        var redisSettings = configuration.GetSection(nameof(RedisSettings)).Get<RedisSettings>();
        ArgumentNullException.ThrowIfNull(redisSettings);
        ArgumentNullException.ThrowIfNull(redisSettings.Configuration);

        services.AddHealthChecks();
        services.AddRedisConfiguration(redisSettings);
        services.AddHangfireConfiguration(redisSettings);
        services.AddHttpClientWithPolicies(configuration);

        // Add exception handler
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        services.AddCronJobs();

        return services;
    }

    private static IServiceCollection AddCronJobs(this IServiceCollection services)
    {
        // Register jobs
        services.AddScoped<CustomerPullJob>();
        services.AddScoped<IdCodePushJob>();

        // Register job dependencies
        services.AddSingleton<IDynamicRepository, DynamicRepository>();
        services.AddSingleton<IDataTrackerRepository, DataTrackerRepository>();
        services.AddSingleton<IDataRepository, DataRepository>();

        // Register shared services
        services.AddSingleton<ILoggerService, LoggerService>();
        services.AddSingleton<ICacheService, RedisCacheService>();
        services.AddSingleton<IDateTimeService, DateTimeService>();

        return services;
    }
}