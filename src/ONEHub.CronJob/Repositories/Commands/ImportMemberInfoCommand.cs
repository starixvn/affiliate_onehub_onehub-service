using Fanex.Data.Repository;

namespace ONEHub.CronJob.Repositories.Commands;

/// <summary>
/// Command for importing member/customer information using XML batch import
/// Matches the pattern from Task_API_Queue_Import
/// </summary>
public class ImportMemberInfoCommand : INonQueryCommand
{
    public string GetSettingKey() => "Task_API_MemberInfo_Import";

    public bool IsValid() => !string.IsNullOrEmpty(XmlData);

    /// <summary>
    /// XML data containing customer records
    /// </summary>
    public string XmlData { get; set; } = string.Empty;

    /// <summary>
    /// Error code (OUTPUT parameter from SP)
    /// </summary>
    public int ErrCode { get; set; }

    /// <summary>
    /// Error message (OUTPUT parameter from SP)
    /// </summary>
    public string ErrMsg { get; set; } = string.Empty;
}

