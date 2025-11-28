using ONEHub.CronJob.Models;
using ONEHub.CronJob.Repositories.Commands;
using ONEHub.CronJob.Repositories.Criterias;

namespace ONEHub.CronJob.Repositories.Interfaces;

public interface IDataTrackerRepository
{
    Task InsertDataTrackerAsync(InsertDataTrackerCommand command);

    Task UpdateDataTrackerAsync(UpdateDataTrackerCommand command);

    Task<IEnumerable<DataTrackerItem>> FetchDataTracker(GetDataTrackerCriteria criteria);
}