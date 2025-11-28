using Hangfire;
using Microsoft.Extensions.Options;
using ONEHub.CronJob.Configurations;
using ONEHub.CronJob.Constants;
using ONEHub.CronJob.Repositories.Commands;
using ONEHub.CronJob.Repositories.Criterias;
using ONEHub.CronJob.Repositories.Interfaces;
using ONEHub.CronJob.Services;
using ONEHub.Shared.Services.Interfaces;

namespace ONEHub.CronJob.Jobs;

[DisableConcurrentExecution(60)]
[AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Fail, Attempts = 0)]
public class IdCodePushJob(
    ILoggerService logger,
    IDateTimeService dateTimeService,
    IDataTrackerRepository dataTrackerRepository,
    IExternalApiClient apiClient,
    IOptions<JobSettings> jobSettings) : JobBase(logger, dateTimeService)
{
    protected override async Task ExecuteAsync()
    {
        // Get job configuration
        var config = jobSettings.Value.GetByName(nameof(IdCodePushJob));

        // Fetch pending items with StatusId = 0 (InProcess) and StatusId = 2 (Failure)
        var inProcessCriteria = new GetDataTrackerCriteria
        {
            StatusId = DataTrackerStatus.InProcess
        };

        var failureCriteria = new GetDataTrackerCriteria
        {
            StatusId = DataTrackerStatus.Failure
        };

        var inProcessItems = await dataTrackerRepository.FetchDataTracker(inProcessCriteria);
        var failureItems = await dataTrackerRepository.FetchDataTracker(failureCriteria);

        // Combine both lists and take MaxBatchSize items
        var trackerItems = inProcessItems.Concat(failureItems);
        var itemsList = trackerItems.Take(config.MaxBatchSize).ToList();

        if (itemsList.Count == 0)
        {
            logger.LogInfo("No pending IdCode tracker items found (InProcess or Failure)");
            return;
        }

        logger.LogInfo($"Found {itemsList.Count} pending IdCode tracker items to process (InProcess or Failure)");

        int successCount = 0;
        int failureCount = 0;

        foreach (var item in itemsList)
        {
            // Call external API with raw JSON string
            var (success, errorMessage) = await apiClient.PushIdCodeAsync(item.PushDataInfo);

            var command = new UpdateDataTrackerCommand
            {
                PushTrackerId = item.PushTrackerId,
                StatusId = success ? DataTrackerStatus.Success : DataTrackerStatus.Failure,
                ErrCode = success ? 0 : -1,
                ErrMsg = success ? string.Empty : errorMessage
            };

            await dataTrackerRepository.UpdateDataTrackerAsync(command);

            if (success)
            {
                successCount++;
            }
            else
            {
                failureCount++;
            }
        }

        logger.LogInfo($"IdCodePushJob completed: {successCount} succeeded, {failureCount} failed");
    }
}
