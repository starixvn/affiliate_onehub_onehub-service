namespace ONEHub.CronJob.Models;

/// <summary>
/// Sample data model for transferring data between services
/// </summary>
public interface IDataTracker
{
    int PushDataType { get; }

    int StatusId { get; }

    long PushTrackerId { get; }
}
