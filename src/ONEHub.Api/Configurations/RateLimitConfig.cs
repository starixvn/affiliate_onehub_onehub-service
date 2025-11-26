using AspNetCoreRateLimit;

namespace ONEHub.Api.Configurations;

/// <summary>
/// Rate limiting configuration extensions
/// </summary>
public static class RateLimitConfig
{
    public static IServiceCollection AddRateLimitConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMemoryCache();

        services.Configure<IpRateLimitOptions>(
            configuration.GetSection("IpRateLimiting"));

        services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
        services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

        return services;
    }
}
