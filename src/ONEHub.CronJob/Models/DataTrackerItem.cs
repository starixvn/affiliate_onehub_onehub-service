namespace ONEHub.Shared.Models;

public class DataTrackerItem
{
    public string PrefixSiteName { get; set; }

    public long PushTrackerId { get; set; }

    public int StatusId { get; set; }

    public int PushDataType { get; set; }

    public string PushDataInfo { get; set; }
}