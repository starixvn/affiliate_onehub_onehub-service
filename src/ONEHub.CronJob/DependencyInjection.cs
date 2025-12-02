using Fanex.Data.Repository;
using ONEHub.CronJob.Configurations;
using ONEHub.CronJob.Jobs;
using ONEHub.CronJob.Jobs.OHCustomer;
using ONEHub.CronJob.Repositories;
using ONEHub.CronJob.Repositories.Interfaces;
using ONEHub.CronJob.Services;
using ONEHub.CronJob.Services.Interfaces;
using ONEHub.Shared.Models;
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

        services.Configure<TokenRequest>(configuration.GetSection(nameof(TokenRequest)));
        services.AddOptions<TokenRequest>().ValidateOnStart();

        // Get RedisSettings from configuration
        var redisSettings = configuration.GetSection(nameof(RedisSettings)).Get<RedisSettings>();
        ArgumentNullException.ThrowIfNull(redisSettings);
        ArgumentNullException.ThrowIfNull(redisSettings.Configuration);

        // Add memory cache for token caching
        services.AddMemoryCache();

        services.AddHealthChecks();
        services.AddFanexData(configuration);
        services.AddRedisConfiguration(redisSettings);
        services.AddHangfireConfiguration(redisSettings);

        // Register token API client (must be before AddHttpClientWithPolicies)
        services.AddTokenApiClient(configuration);
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
        services.AddScoped<OHCustomerPullJob>();
        services.AddScoped<OHCustomerRealTimePullJob>();
        services.AddScoped<OHCustomerManualPullJob>();
        services.AddScoped<IdCodePushJob>();

        // Register job dependencies
        services.AddSingleton<IDynamicRepository, DynamicRepository>();
        services.AddSingleton<IDataTrackerRepository, DataTrackerRepository>();
        services.AddSingleton<IDataRepository, DataRepository>();

        // Register shared services
        services.AddSingleton<ILoggerService, LoggerService>();
        services.AddSingleton<ICacheService, RedisCacheService>();
        services.AddSingleton<IDateTimeService, DateTimeService>();

        // Register API clients
        services.AddScoped<IExternalApiClient, ExternalApiClient>();
        services.AddScoped<ITokenApi, TokenApiService>();

        return services;
    }
}