using Fanex.Data;
using Fanex.Data.Repository;
using ONEHub.CronJob.Constants;
using ONEHub.Shared.Models;

namespace ONEHub.CronJob.Repositories.Commands;

public class InsertDataTrackerCommand : INonQueryCommand, IDataTracker
{
    public string GetSettingKey() => "Task_API_PushDataTracker_Import";

    public bool IsValid() => true;

    public int PushDataType { get; set; }

    public long PushTrackerId { get; set; }

    [SpParam(Ignored = true)]
    public int StatusId => DataTrackerStatus.InProcess;

    public int ErrCode { get; set; }

    public string ErrMsg { get; set; } = string.Empty;
}