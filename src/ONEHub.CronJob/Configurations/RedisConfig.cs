using StackExchange.Redis;

namespace ONEHub.CronJob.Configurations;

/// <summary>
/// Redis configuration settings
/// </summary>
public class RedisSettings
{
    public string InstanceName { get; set; } = null!;
    public string Configuration { get; set; } = null!;
}

/// <summary>
/// Redis configuration extensions
/// </summary>
public static class RedisConfig
{
    public static IServiceCollection AddRedisConfiguration(
        this IServiceCollection services,
        RedisSettings redisSettings)
    {
        ArgumentNullException.ThrowIfNull(redisSettings);
        ArgumentNullException.ThrowIfNull(redisSettings.Configuration);

        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(redisSettings.Configuration));

        services.AddHealthChecks()
            .AddRedis(redisSettings.Configuration, name: "redis");

        return services;
    }
}
