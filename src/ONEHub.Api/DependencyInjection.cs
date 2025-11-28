using ONEHub.Api.Configurations;
using ONEHub.Shared.Services;
using ONEHub.Shared.Services.Interfaces;

namespace ONEHub.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddDependencyInjection(this IServiceCollection services, IConfiguration configuration)
    {
        // Health checks
        services.AddHealthChecks();

        // Configure Redis
        services.AddRedisConfiguration(configuration);

        // Configure rate limiting
        services.AddRateLimitConfiguration(configuration);

        // Configure HTTP clients with Polly policies
        services.AddPollyPolicies(configuration);

        // Configure Swagger/OpenAPI
        services.AddSwaggerConfiguration();

        // Add exception handler
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        // Register shared services
        services.AddSingleton<ILoggerService, LoggerService>();
        services.AddSingleton<IDateTimeService, DateTimeService>();

        return services;
    }
}