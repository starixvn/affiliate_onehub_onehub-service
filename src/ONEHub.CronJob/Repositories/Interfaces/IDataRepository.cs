using ONEHub.CronJob.Models;
using ONEHub.CronJob.Repositories.Criterias;

namespace ONEHub.CronJob.Repositories.Interfaces;
public interface IDataRepository
{
    Task<IEnumerable<IdCodeInfo>> GetIdCodeInfoAsync(GetIdCodeInfoCriteria criteria);
}
