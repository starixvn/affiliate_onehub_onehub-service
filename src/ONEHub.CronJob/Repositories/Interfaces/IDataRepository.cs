namespace ONEHub.CronJob.Repositories.Interfaces;

using Criterias;
using Models;

public interface IDataRepository
{
    Task<IEnumerable<IdCodeInfo>> GetIdCodeInfoAsync(GetIdCodeInfoCriteria criteria);
}