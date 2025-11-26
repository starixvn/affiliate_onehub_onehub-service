namespace ONEHub.CronJob.Repositories;

using Commands;
using Criterias;
using Fanex.Data.Repository;
using Interfaces;
using Shared.Models;

public class DataTrackerRepository(IDynamicRepository repo) : IDataTrackerRepository
{
    public Task InsertDataTrackerAsync(InsertDataTrackerCommand command)
        => repo.ExecuteAsync(command);

    public Task UpdateDataTrackerAsync(UpdateDataTrackerCommand command)
        => repo.ExecuteAsync(command);

    public Task<IEnumerable<DataTrackerItem>> FetchDataTracker(GetDataTrackerCriteria criteria)
        => repo.FetchAsync<DataTrackerItem>(criteria);
}