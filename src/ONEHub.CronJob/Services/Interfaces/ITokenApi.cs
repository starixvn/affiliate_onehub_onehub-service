using ONEHub.CronJob.Models;
using ONEHub.Shared.Models;

namespace ONEHub.CronJob.Services.Interfaces;

/// <summary>
/// Interface for token authentication API
/// </summary>
public interface ITokenApi
{
    /// <summary>
    /// Gets bearer token from authentication API
    /// </summary>
    /// <param name="request">Token request with username and password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Token response with access token</returns>
    Task<TokenResponse> GetTokenAsync(TokenRequest request, CancellationToken cancellationToken = default);
}

