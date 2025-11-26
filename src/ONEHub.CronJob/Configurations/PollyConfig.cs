using Polly;
using Polly.Extensions.Http;
using System.Net;

namespace ONEHub.CronJob.Configurations;

/// <summary>
/// Polly resilience policies configuration
/// </summary>
public static class PollyConfig
{
    public static IServiceCollection AddHttpClientWithPolicies(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var baseUrl = configuration["ExternalApi:BaseUrl"] ?? "https://api.example.com";
        var proxyEnabled = configuration.GetValue<bool>("ExternalApi:Proxy:Enabled");
        var proxyHost = configuration["ExternalApi:Proxy:Host"];

        var httpClientBuilder = services.AddHttpClient("ExternalApi", client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // Configure proxy if enabled
        if (proxyEnabled && !string.IsNullOrEmpty(proxyHost))
        {
            httpClientBuilder.ConfigurePrimaryHttpMessageHandler(() =>
            {
                return new HttpClientHandler
                {
                    Proxy = new WebProxy(proxyHost),
                    UseProxy = true
                };
            });
        }

        httpClientBuilder
            .AddPolicyHandler(HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(new[]
                {
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(5),
                    TimeSpan.FromSeconds(10)
                }))
            .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(30));

        return services;
    }
}
