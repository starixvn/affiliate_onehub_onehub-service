namespace ONEHub.CronJob.Repositories.Interfaces;

using Commands;
using Criterias;
using Shared.Models;

public interface IDataTrackerRepository
{
    Task InsertDataTrackerAsync(InsertDataTrackerCommand command);

    Task UpdateDataTrackerAsync(UpdateDataTrackerCommand command);

    Task<IEnumerable<DataTrackerItem>> FetchDataTracker(GetDataTrackerCriteria criteria);
}