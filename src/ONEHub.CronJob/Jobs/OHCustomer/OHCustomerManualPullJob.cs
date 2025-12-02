using Microsoft.Extensions.Options;
using ONEHub.CronJob.Configurations;
using ONEHub.CronJob.Repositories.Interfaces;
using ONEHub.CronJob.Services;
using ONEHub.Shared.Services.Interfaces;

namespace ONEHub.CronJob.Jobs.OHCustomer;

/// <summary>
/// Manual customer data pull job
/// Does not run automatically (scheduled for Feb 31st which doesn't exist)
/// Designed to be triggered manually via API for backfills or specific date ranges
/// Uses a 60-minute time range for larger data pulls
/// </summary>
public class OHCustomerManualPullJob(
    ILoggerService logger,
    IDateTimeService dateTimeService,
    IExternalApiClient apiClient,
    IDataRepository dataRepository,
    IOptions<JobSettings> jobSettings) : BaseOHCustomerPullJob(logger, dateTimeService, apiClient, dataRepository, jobSettings)
{
    protected override string JobName => nameof(OHCustomerManualPullJob);
}

