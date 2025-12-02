using ONEHub.CronJob.Models;
using ONEHub.CronJob.Repositories.Commands;
using ONEHub.CronJob.Repositories.Criterias;

namespace ONEHub.CronJob.Repositories.Interfaces;

public interface IDataRepository
{
    Task<IEnumerable<IdCodeInfo>> GetIdCodeInfoAsync(GetIdCodeInfoCriteria criteria);

    Task<DateTime?> GetLastImportedTimeAsync(GetMemberInfoImportedTimeCriteria criteria);

    Task ImportMemberInfoAsync(ImportMemberInfoCommand command);

    Task UpdateMemberInfoImportedTimeAsync(UpdateMemberInfoImportedTimeCommand command);
}
