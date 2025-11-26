namespace ONEHub.Shared.Models;

/// <summary>
/// Sample data model for transferring data between services
/// </summary>
public interface IDataTracker
{
    public int PushDataType { get; }

    public int StatusId { get; }

    public long PushTrackerId { get; }
}