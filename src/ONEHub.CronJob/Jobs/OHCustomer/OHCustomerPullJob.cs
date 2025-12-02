using Microsoft.Extensions.Options;
using ONEHub.CronJob.Configurations;
using ONEHub.CronJob.Repositories.Interfaces;
using ONEHub.CronJob.Services;
using ONEHub.Shared.Services.Interfaces;

namespace ONEHub.CronJob.Jobs.OHCustomer;

/// <summary>
/// Standard customer data pull job
/// Runs every 30 minutes with a 30-minute time range
/// </summary>
public class OHCustomerPullJob(
    ILoggerService logger,
    IDateTimeService dateTimeService,
    IExternalApiClient apiClient,
    IDataRepository dataRepository,
    IOptions<JobSettings> jobSettings) : BaseOHCustomerPullJob(logger, dateTimeService, apiClient, dataRepository, jobSettings)
{
    protected override string JobName => nameof(OHCustomerPullJob);
}
