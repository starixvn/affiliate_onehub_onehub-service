using Fanex.Data;
using Fanex.Data.Repository;
using ONEHub.Shared.Models;

namespace ONEHub.CronJob.Repositories.Commands;

public class UpdateDataTrackerCommand : INonQueryCommand, IDataTracker
{
    public string GetSettingKey() => "Task_API_PushDataTracker_Upd";

    public bool IsValid() => true;

    [SpParam(Ignored = true)]
    public int PushDataType { get; set; }

    public long PushTrackerId { get; set; }

    public int StatusId { get; set; }

    public int ErrCode { get; set; }

    public string ErrMsg { get; set; } = string.Empty;
}