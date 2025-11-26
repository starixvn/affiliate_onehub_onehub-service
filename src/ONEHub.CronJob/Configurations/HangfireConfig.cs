using Hangfire;
using Hangfire.Dashboard;
using Hangfire.Redis.StackExchange;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace ONEHub.CronJob.Configurations;

/// <summary>
/// Hangfire configuration extensions
/// </summary>
public static class HangfireConfig
{
    public static IServiceCollection AddHangfireConfiguration(
        this IServiceCollection services,
        RedisSettings redisSettings)
    {
        ArgumentNullException.ThrowIfNull(redisSettings);

        services.AddHangfire((serviceProvider, config) =>
        {
            var redis = serviceProvider.GetRequiredService<IConnectionMultiplexer>();
            config.UseRedisStorage(redis, new RedisStorageOptions
            {
                Prefix = redisSettings.InstanceName
            });
        });

        services.AddHangfireServer(options =>
        {
            options.WorkerCount = 2;
            options.ServerName = "ONEHub-CronJob-Server";
        });

        return services;
    }

    public static IApplicationBuilder UseHangfireConfiguration(this WebApplication app)
    {
        var hangfireSettings = app.Services.GetRequiredService<IOptions<HangfireSettings>>();

        ArgumentNullException.ThrowIfNull(hangfireSettings);

        var settings = hangfireSettings.Value;

        app.UseHangfireDashboard(pathMatch: settings.DashboardPath, options: new DashboardOptions
        {
            Authorization = [new HangfireAuthorizationFilter(settings.DashboardSecretKey)]
        });

        return app;
    }
}

/// <summary>
/// Hangfire configuration settings
/// </summary>
public class HangfireSettings
{
    /// <summary>
    /// Secret key for Hangfire dashboard authentication (optional)
    /// If empty or null, dashboard will be accessible without authentication
    /// </summary>
    public string? DashboardSecretKey { get; set; }

    /// <summary>
    /// Dashboard URL path (default: /hangfire)
    /// </summary>
    public string DashboardPath { get; set; } = "/hangfire";
}

/// <summary>
/// Authorization filter for Hangfire Dashboard
/// </summary>
public class HangfireAuthorizationFilter(string? secretKey) : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        if (string.IsNullOrEmpty(secretKey))
        {
            return true;
        }

        var httpContext = context.GetHttpContext();
        var key = httpContext.Request.Query["key"].ToString();

        if (string.IsNullOrEmpty(key))
        {
            return false;
        }

        return key == secretKey;
    }
}