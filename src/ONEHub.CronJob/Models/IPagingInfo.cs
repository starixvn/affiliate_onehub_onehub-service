namespace ONEHub.CronJob.Models;

public interface IPagingInfo
{
    int PageIndex { get; }

    int PageSize { get; }
}