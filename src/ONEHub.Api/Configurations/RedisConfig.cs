using StackExchange.Redis;

namespace ONEHub.Api.Configurations;

/// <summary>
/// Redis configuration extensions
/// </summary>
public static class RedisConfig
{
    public static IServiceCollection AddRedisConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var redisConnection = configuration.GetConnectionString("Redis") ?? "localhost:6379";

        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect(redisConnection));

        services.AddHealthChecks()
            .AddRedis(redisConnection, name: "redis");

        return services;
    }
}
