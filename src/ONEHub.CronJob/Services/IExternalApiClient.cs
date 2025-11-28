namespace ONEHub.CronJob.Services;

/// <summary>
/// External API client for HTTP communication with external services
/// </summary>
public interface IExternalApiClient
{
    /// <summary>
    /// Pushes IdCode data to external API
    /// </summary>
    /// <param name="jsonData">JSON string containing IdCode data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tuple indicating success status and error message if failed</returns>
    Task<(bool Success, string ErrorMessage)> PushIdCodeAsync(string jsonData, CancellationToken cancellationToken = default);
}
