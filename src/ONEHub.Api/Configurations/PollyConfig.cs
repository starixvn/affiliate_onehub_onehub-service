namespace ONEHub.Api.Configurations;

/// <summary>
/// Polly resilience policies configuration
/// </summary>
public static class PollyConfig
{
    public static IServiceCollection AddPollyPolicies(
        this IServiceCollection services,
        IConfiguration _)
    {
        // Policies are applied when configuring specific HttpClients
        // This is a placeholder for centralized policy management

        return services;
    }
}
