namespace ONEHub.CronJob.Repositories.Criterias;

using Fanex.Data;
using Fanex.Data.Repository;
using Shared.Models;

public class GetDataTrackerCriteria : IDataTracker, ICriteria
{
    public string GetSettingKey() => "Task_API_PushDataTracker_Sel";

    public bool IsValid() => true;

    public int PushDataType { get; set; }

    public int StatusId { get; set; }

    [SpParam(Ignored = true)]
    public long PushTrackerId => 0;
}