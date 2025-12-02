using System.Text;
using System.Text.Json;
using ONEHub.CronJob.Models;
using ONEHub.Shared.Services.Interfaces;

namespace ONEHub.CronJob.Services;

/// <summary>
/// Implementation of external API client for external service communication
/// </summary>
public class ExternalApiClient : IExternalApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILoggerService _logger;
    private const string _idCodePushPath = "api/customer/insertidcode";
    private const string _customerPullPath = "api/customer/getohcustomers";

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ExternalApiClient(IHttpClientFactory httpClientFactory, ILoggerService logger)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

    /// <summary>
    /// Pulls customer information from external API with pagination
    /// </summary>
    /// <param name="fromDate">Start date for customer data</param>
    /// <param name="toDate">End date for customer data</param>
    /// <param name="pageIndex">Page number (1-based)</param>
    /// <param name="pageSize">Number of records per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of customer information objects for the requested page</returns>
    public async Task<List<OHCustomerInfo>> GetOHCustomersAsync(
        DateTime fromDate,
        DateTime toDate,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get HttpClient configured with Polly retry, proxy, and authentication
            var client = _httpClientFactory.CreateClient("ExternalApi");

            // Build query string with parameters
            var queryString = $"?fromDate={fromDate:yyyy-MM-dd HH:mm:ss}&toDate={toDate:yyyy-MM-dd HH:mm:ss}&pageIndex={pageIndex}&pageSize={pageSize}";
            var requestUri = $"{_customerPullPath}{queryString}";

            var url = $"{client.BaseAddress?.ToString().TrimEnd('/')}/{requestUri}";
            _logger.LogInfo($"Pulling customer data from {url}");

            // GET request with query parameters
            var response = await client.GetAsync(requestUri, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var errorMessage = $"HTTP {(int)response.StatusCode}: {errorContent}";
                _logger.LogError($"Failed to pull customer data - {errorMessage}");
                throw new HttpRequestException($"Failed to retrieve customer data. Status: {response.StatusCode}, Details: {errorContent}");
            }

            // Deserialize response to List<OHCustomerInfo>
            var customers = await response.Content.ReadFromJsonAsync<List<OHCustomerInfo>>(_jsonOptions, cancellationToken);

            if (customers == null)
            {
                _logger.LogInfo("API returned null customer list, returning empty list");
                return new List<OHCustomerInfo>();
            }

            _logger.LogInfo($"Successfully pulled {customers.Count} customer(s) from API");
            return customers;
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError("Timeout pulling customer data", ex);
            throw new TimeoutException("Request to pull customer data timed out", ex);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError($"HTTP error pulling customer data: {ex.Message}", ex);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError($"Failed to deserialize customer data: {ex.Message}", ex);
            throw new InvalidOperationException("Failed to parse customer data from API response", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error pulling customer data: {ex.Message}", ex);
            throw;
        }
    }
}
