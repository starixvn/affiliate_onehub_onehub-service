using Fanex.Data.Repository;
using ONEHub.CronJob.Models;
using ONEHub.CronJob.Repositories.Criterias;
using ONEHub.CronJob.Repositories.Interfaces;

namespace ONEHub.CronJob.Repositories;
public class DataRepository(IDynamicRepository repo) : IDataRepository
{
    public Task<IEnumerable<IdCodeInfo>> GetIdCodeInfoAsync(GetIdCodeInfoCriteria criteria)
        => repo.FetchAsync<IdCodeInfo>(criteria);
}
