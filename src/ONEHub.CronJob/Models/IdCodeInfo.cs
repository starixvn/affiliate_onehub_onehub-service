namespace ONEHub.CronJob.Models;

public class IdCodeInfo
{
    public int RowIdx { get; set; }

    public int Id { get; set; }

    public string IdCode { get; set; } = string.Empty;

    public int StatusId { get; set; }

    public DateTime CreatedDate { get; set; }

    public int SiteId { get; set; }
}