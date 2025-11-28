using Fanex.Data;
using Fanex.Data.Vault;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ONEHub.CronJob.Configurations;

public static class DatabaseConfig
{
    public static IServiceCollection AddFanexData(
           this IServiceCollection services,
           IConfiguration configuration)
    {
        DbSettingProviderManager
                    .StartNewSession()
                    .Register(configuration)
                    .Run();

        services.TryAddSingleton<VaultDbAliasService, VaultDbAliasServiceImp>();
        return services;
    }
}
