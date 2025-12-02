namespace ONEHub.CronJob.Models;

public class OHCustomerInfo
{
    public int SiteId { get; set; }

    public string SiteName { get; set; } = string.Empty;

    public int CustId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public int? CurrencyId { get; set; }

    public int? ActStatus { get; set; }

    public DateTime? RegTime { get; set; }

    public DateTime? FirstDepositDate { get; set; }

    public int? Status { get; set; }

    public bool? IsTest { get; set; }

    public string IDCode { get; set; } = string.Empty;

    public long? ClickId { get; set; }
}
