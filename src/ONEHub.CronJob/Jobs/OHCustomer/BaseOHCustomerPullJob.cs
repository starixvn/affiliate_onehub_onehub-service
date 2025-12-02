using System.Data.SqlTypes;
using Hangfire;
using Microsoft.Extensions.Options;
using ONEHub.CronJob.Configurations;
using ONEHub.CronJob.Helpers;
using ONEHub.CronJob.Models;
using ONEHub.CronJob.Repositories.Commands;
using ONEHub.CronJob.Repositories.Criterias;
using ONEHub.CronJob.Repositories.Interfaces;
using ONEHub.CronJob.Services;
using ONEHub.Shared.Services.Interfaces;

namespace ONEHub.CronJob.Jobs.OHCustomer;

/// <summary>
/// Base class for customer data pull jobs
/// Contains shared logic for fetching customer data from external API and importing to database
/// </summary>
[DisableConcurrentExecution(60)]
[AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Fail, Attempts = 0)]
public abstract class BaseOHCustomerPullJob(
    ILoggerService logger,
    IDateTimeService dateTimeService,
    IExternalApiClient apiClient,
    IDataRepository dataRepository,
    IOptions<JobSettings> jobSettings) : JobBase(logger, dateTimeService)
{
    protected readonly ILoggerService logger = logger;
    protected readonly IDateTimeService dateTimeService = dateTimeService;
    protected readonly IExternalApiClient _apiClient = apiClient;
    protected readonly IDataRepository _dataRepository = dataRepository;
    protected readonly IOptions<JobSettings> _jobSettings = jobSettings;

    /// <summary>
    /// Gets the job name for configuration lookup
    /// Must be implemented by derived classes
    /// </summary>
    protected abstract string JobName { get; }

    /// <summary>
    /// Main execution logic - pulls customer data and imports to database
    /// </summary>
    protected override async Task ExecuteAsync()
    {
        // Get job configuration
        var config = _jobSettings.Value.GetByName(JobName);

        logger.LogInfo($"Starting {JobName} with DataSource: {config.DataSource}");

        // Get last imported time from database
        var fromDate = await GetLastImportedTimeAsync(config.DataSource);

        // Get current time as end of range
        var toDate = dateTimeService.UtcNow;

        // Split large time range into smaller chunks to prevent timeouts and track progress
        // GetProportionTimeRange handles everything - no need to limit the range beforehand
        var maxTimeRangeSpan = TimeSpan.FromMinutes(config.MaxTimeRange);
        var dateRanges = DateTimeHelper.GetProportionTimeRange(fromDate, toDate, maxTimeRangeSpan, isExcludeNotFullRangeSpan: false);

        logger.LogInfo($"Pulling customers from {fromDate:yyyy-MM-dd HH:mm:ss} to {toDate:yyyy-MM-dd HH:mm:ss}");

        int totalProcessed = 0;

        try
        {
            // Process each time range separately
            foreach (var range in dateRanges)
            {
                logger.LogInfo($"Processing range: {range.FromDate:yyyy-MM-dd HH:mm:ss} to {range.ToDate:yyyy-MM-dd HH:mm:ss}");

                // Import all data for this time range (with pagination)
                var rangeProcessed = await ProcessTimeRangeAsync(range.FromDate, range.ToDate, config.MaxBatchSize, config.DataSource);

                totalProcessed += rangeProcessed;

                // CRITICAL: Update tracker after EACH range is successfully imported
                // This ensures progress is saved even if later ranges fail
                await UpdateLastImportedTimeAsync(config.DataSource, range.ToDate);
                logger.LogInfo($"Updated tracker to {range.ToDate:yyyy-MM-dd HH:mm:ss} after importing {rangeProcessed} customers");
            }

            logger.LogInfo($"{JobName} completed successfully: {totalProcessed} customers imported");
        }
        catch (TimeoutException ex)
        {
            logger.LogError($"Timeout while pulling customer data: {ex.Message}", ex);
            throw; // Let Hangfire handle retry
        }
        catch (HttpRequestException ex)
        {
            logger.LogError($"HTTP error while pulling customer data: {ex.Message}", ex);
            throw; // Let Hangfire handle retry
        }
        catch (Exception ex)
        {
            logger.LogError($"Unexpected error while pulling customer data: {ex.Message}", ex);
            throw; // Let Hangfire handle retry
        }
    }

    /// <summary>
    /// Processes a single time range with pagination
    /// Returns the number of customers imported
    /// </summary>
    protected virtual async Task<int> ProcessTimeRangeAsync(
        DateTime fromDate,
        DateTime toDate,
        int batchSize,
        string dataSource)
    {
        int totalProcessed = 0;
        int pageIndex = 0;
        bool hasMoreData = true;

        while (hasMoreData)
        {
            // Fetch customer data from API
            var customers = await FetchCustomersFromApiAsync(fromDate, toDate, pageIndex, batchSize);

            if (customers == null || customers.Count == 0)
            {
                logger.LogInfo($"No more customers found on page {pageIndex}");
                break;
            }

            logger.LogInfo($"Retrieved {customers.Count} customer(s) on page {pageIndex}");

            // Import customers to database
            await ImportCustomerBatchAsync(customers);

            totalProcessed += customers.Count;
            logger.LogInfo($"Successfully imported batch of {customers.Count} customers");

            // Check if there's more data
            hasMoreData = customers.Count >= batchSize;
            pageIndex++;
        }

        return totalProcessed;
    }

    /// <summary>
    /// Gets the last imported time from database
    /// </summary>
    protected virtual async Task<DateTime> GetLastImportedTimeAsync(string dataSource)
    {
        var criteria = new GetMemberInfoImportedTimeCriteria
        {
            DataSource = dataSource
        };

        var result = await _dataRepository.GetLastImportedTimeAsync(criteria);

        // If no previous import, use a default starting point
        if (!result.HasValue)
        {
            logger.LogInfo($"No previous import found for {dataSource}, starting from default date");
            return dateTimeService.UtcNow.AddDays(-1); // Default: start from yesterday
        }

        return result.Value;
    }

    /// <summary>
    /// Fetches customers from external API with pagination using GET request
    /// </summary>
    protected virtual async Task<List<OHCustomerInfo>> FetchCustomersFromApiAsync(
        DateTime fromDate,
        DateTime toDate,
        int pageIndex,
        int pageSize)
    {
        logger.LogInfo($"Fetching customers: fromDate={fromDate:yyyy-MM-dd HH:mm:ss}, toDate={toDate:yyyy-MM-dd HH:mm:ss}, pageIndex={pageIndex}, pageSize={pageSize}");

        // API uses GET with query parameters
        var customers = await _apiClient.GetOHCustomersAsync(fromDate, toDate, pageIndex, pageSize);

        return customers ?? new List<OHCustomerInfo>();
    }

    /// <summary>
    /// Imports a batch of customers to database using XML batch import
    /// </summary>
    protected virtual async Task ImportCustomerBatchAsync(
        List<OHCustomerInfo> customers)
    {
        // Build XML from customer data
        var xmlData = XmlHelper.BuildXml(customers);

        // Import customer data using batch XML import
        var importCommand = new ImportMemberInfoCommand
        {
            XmlData = xmlData
        };

        await _dataRepository.ImportMemberInfoAsync(importCommand);

        // Check for errors from stored procedure
        if (importCommand.ErrCode != 0)
        {
            logger.LogError($"Import failed with error code {importCommand.ErrCode}: {importCommand.ErrMsg}");
            throw new SqlTypeException($"Import failed: {importCommand.ErrMsg}");
        }
    }

    /// <summary>
    /// Updates the last imported time in database after successful import
    /// </summary>
    protected virtual async Task UpdateLastImportedTimeAsync(string dataSource, DateTime toDate)
    {
        var updateTimeCommand = new UpdateMemberInfoImportedTimeCommand
        {
            DataSource = dataSource,
            LastImportedTime = toDate
        };

        await _dataRepository.UpdateMemberInfoImportedTimeAsync(updateTimeCommand);

        if (updateTimeCommand.ErrCode != 0)
        {
            logger.LogError($"Updated last imported time failed with error code {updateTimeCommand.ErrCode}: {updateTimeCommand.ErrMsg}");
            throw new SqlTypeException($"Updated last imported time failed: {updateTimeCommand.ErrMsg}");
        }
    }
}

