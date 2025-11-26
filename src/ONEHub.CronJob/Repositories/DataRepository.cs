namespace ONEHub.CronJob.Repositories;

using Criterias;
using Fanex.Data.Repository;
using Interfaces;
using Models;

public class DataRepository(IDynamicRepository repo) : IDataRepository
{
    public Task<IEnumerable<IdCodeInfo>> GetIdCodeInfoAsync(GetIdCodeInfoCriteria criteria)
        => repo.FetchAsync<IdCodeInfo>(criteria);
}