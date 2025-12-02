using System.Text.Json;
using ONEHub.CronJob.Models;
using ONEHub.CronJob.Services.Interfaces;
using ONEHub.Shared.Models;
using ONEHub.Shared.Services.Interfaces;

namespace ONEHub.CronJob.Services;

/// <summary>
/// Service for fetching authentication tokens from external API
/// </summary>
public class TokenApiService : ITokenApi
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILoggerService _logger;
    private const string TokenApiClientName = "TokenApi";
    private const string TokenEndpoint = "/api/auth";

    public TokenApiService(IHttpClientFactory httpClientFactory, ILoggerService logger)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets bearer token from authentication API
    /// </summary>
    public async Task<TokenResponse> GetTokenAsync(TokenRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient(TokenApiClientName);

            _logger.LogInfo($"Requesting token for user: {request.UserName}");

            // Create request payload
            var payload = new
            {
                username = request.UserName,
                password = request.Password
            };

            // POST to token endpoint
            var response = await client.PostAsJsonAsync(TokenEndpoint, payload, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError($"Failed to get token - HTTP {(int)response.StatusCode}: {errorContent}");
                throw new HttpRequestException($"Token {nameof(request)} failed with status {response.StatusCode}: {errorContent}");
            }

            var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken);

            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
            {
                _logger.LogError("Token response is null or empty");
                throw new InvalidOperationException("Invalid token response received from API");
            }

            _logger.LogInfo("Successfully obtained authentication token");
            return tokenResponse;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError($"HTTP error while fetching token: {ex.Message}");
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError($"Failed to deserialize token response: {ex.Message}");
            throw new InvalidOperationException("Failed to parse token response", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error while fetching token: {ex.Message}");
            throw;
        }
    }
}

