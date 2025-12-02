using Fanex.Data;
using Fanex.Data.Repository;
using ONEHub.CronJob.Models;

namespace ONEHub.CronJob.Repositories.Criterias;

public class GetDataTrackerCriteria : IDataTracker, ICriteria
{
    private const int PUSH_IDCODE_TYPE = 1;
    public string GetSettingKey() => "Task_API_PushDataTracker_Sel";

    public bool IsValid() => true;

    public int PushDataType => PUSH_IDCODE_TYPE;

    public int StatusId { get; set; }

    [SpParam(Ignored = true)]
    public long PushTrackerId { get; set; }
}
