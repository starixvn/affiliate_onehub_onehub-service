using Fanex.Data.Repository;
using ONEHub.CronJob.Models;
using ONEHub.CronJob.Repositories.Commands;
using ONEHub.CronJob.Repositories.Criterias;
using ONEHub.CronJob.Repositories.Interfaces;

namespace ONEHub.CronJob.Repositories;

public class DataRepository(IDynamicRepository repo) : IDataRepository
{
    public Task<IEnumerable<IdCodeInfo>> GetIdCodeInfoAsync(GetIdCodeInfoCriteria criteria)
        => repo.FetchAsync<IdCodeInfo>(criteria);

    public Task<DateTime?> GetLastImportedTimeAsync(GetMemberInfoImportedTimeCriteria criteria)
        => repo.GetAsync<DateTime?>(criteria);

    public Task ImportMemberInfoAsync(ImportMemberInfoCommand command)
        => repo.ExecuteAsync(command);

    public Task UpdateMemberInfoImportedTimeAsync(UpdateMemberInfoImportedTimeCommand command)
        => repo.ExecuteAsync(command);
}
