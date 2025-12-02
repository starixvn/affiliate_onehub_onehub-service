using Microsoft.Extensions.Options;
using ONEHub.CronJob.Configurations;
using ONEHub.CronJob.Repositories.Interfaces;
using ONEHub.CronJob.Services;
using ONEHub.Shared.Services.Interfaces;

namespace ONEHub.CronJob.Jobs.OHCustomer;

/// <summary>
/// Real-time customer data pull job
/// Runs every 5 minutes with a 5-minute time range for near real-time synchronization
/// </summary>
public class OHCustomerRealTimePullJob(
    ILoggerService logger,
    IDateTimeService dateTimeService,
    IExternalApiClient apiClient,
    IDataRepository dataRepository,
    IOptions<JobSettings> jobSettings) : BaseOHCustomerPullJob(logger, dateTimeService, apiClient, dataRepository, jobSettings)
{
    protected override string JobName => nameof(OHCustomerRealTimePullJob);
}

