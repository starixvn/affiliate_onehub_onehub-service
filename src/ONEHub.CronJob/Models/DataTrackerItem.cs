namespace ONEHub.CronJob.Models;

public class DataTrackerItem
{
    public long PushTrackerId { get; set; }

    public DateTime FinishedTime { get; set; }

    public int StatusId { get; set; }

    public int PushDataType { get; set; }

    public string PushDataInfo { get; set; } = string.Empty;
}