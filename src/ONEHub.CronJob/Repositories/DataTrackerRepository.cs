using Fanex.Data.Repository;
using ONEHub.CronJob.Models;
using ONEHub.CronJob.Repositories.Commands;
using ONEHub.CronJob.Repositories.Criterias;
using ONEHub.CronJob.Repositories.Interfaces;

namespace ONEHub.CronJob.Repositories;

public class DataTrackerRepository(IDynamicRepository repo) : IDataTrackerRepository
{
    public Task InsertDataTrackerAsync(InsertDataTrackerCommand command)
        => repo.ExecuteAsync(command);

    public Task UpdateDataTrackerAsync(UpdateDataTrackerCommand command)
        => repo.ExecuteAsync(command);

    public Task<IEnumerable<DataTrackerItem>> FetchDataTracker(GetDataTrackerCriteria criteria)
        => repo.FetchAsync<DataTrackerItem>(criteria);
}