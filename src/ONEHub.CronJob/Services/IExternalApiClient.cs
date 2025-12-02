using ONEHub.CronJob.Models;

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

    /// <summary>
    /// Gets customer information from external API with pagination
    /// </summary>
    /// <param name="fromDate">Start date for customer data</param>
    /// <param name="toDate">End date for customer data</param>
    /// <param name="pageIndex">Page number (1-based)</param>
    /// <param name="pageSize">Number of records per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of customer information objects for the requested page</returns>
    Task<List<OHCustomerInfo>> GetOHCustomersAsync(
        DateTime fromDate,
        DateTime toDate,
        int pageIndex,
        int pageSize,
        CancellationToken cancellationToken = default);
}
