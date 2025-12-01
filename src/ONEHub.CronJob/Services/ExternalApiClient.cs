using System.Text;
using ONEHub.Shared.Services.Interfaces;

namespace ONEHub.CronJob.Services;

/// <summary>
/// Implementation of external API client for IdCode push operations
/// </summary>
public class ExternalApiClient : IExternalApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILoggerService _logger;
    private const string _idCodePushPath = "api/v1/idcode/push";

    public ExternalApiClient(IHttpClientFactory httpClientFactory, ILoggerService logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<(bool Success, string ErrorMessage)> PushIdCodeAsync(
        string jsonData,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get HttpClient configured with Polly retry and proxy
            var client = _httpClientFactory.CreateClient("ExternalApi");

            // Combine BaseUrl with hardcoded relative path
            var url = $"{client.BaseAddress?.ToString().TrimEnd('/')}/{_idCodePushPath}";

            _logger.LogInfo($"Pushing IdCode data to {url}");

            // POST raw JSON string
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(_idCodePushPath, content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInfo("Successfully pushed IdCode data");
                return (true, string.Empty);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var errorMessage = $"HTTP {(int)response.StatusCode}: {errorContent}";
                _logger.LogError($"Failed to push IdCode data - {errorMessage}");
                return (false, errorMessage);
            }
        }
        catch (TaskCanceledException ex)
        {
            const string? errorMessage = "Request timeout";
            _logger.LogError("Timeout pushing IdCode data", ex);
            return (false, errorMessage);
        }
        catch (HttpRequestException ex)
        {
            var errorMessage = $"HTTP request error: {ex.Message}";
            _logger.LogError("HTTP error pushing IdCode data", ex);
            return (false, errorMessage);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Unexpected error: {ex.Message}";
            _logger.LogError("Unexpected error pushing IdCode data", ex);
            return (false, errorMessage);
        }
    }
}
