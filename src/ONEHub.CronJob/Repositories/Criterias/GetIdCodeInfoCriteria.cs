using Fanex.Data.Repository;
using ONEHub.CronJob.Models;

namespace ONEHub.CronJob.Repositories.Criterias;
public class GetIdCodeInfoCriteria : ICriteria, IPagingInfo
{
    public string GetSettingKey() => "OH_IDCode_Sel";

    public bool IsValid() => ToDate >= FromDate && PageSize > 0 && PageIndex > 0;

    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }

    public int PageIndex { get; set; }

    public int PageSize { get; set; }
}
