using Fanex.Data;
using Fanex.Data.Repository;

namespace ONEHub.CronJob.Repositories.Commands;

/// <summary>
/// Command for updating last imported time for member data
/// Used to track the last successful import timestamp
/// </summary>
public class UpdateMemberInfoImportedTimeCommand : INonQueryCommand
{
    public string GetSettingKey() => "Task_API_MemberInfo_ImportedTime";

    public bool IsValid() => !string.IsNullOrEmpty(DataSource) && LastImportedTime != default;

    /// <summary>
    /// Data source identifier (e.g., "MemberInfoLastImportedTime")
    /// </summary>
    [SpParam(SqlParamName = "SName")]
    public string DataSource { get; set; } = string.Empty;

    /// <summary>
    /// Last imported timestamp to save
    /// </summary>
    public DateTime LastImportedTime { get; set; }

    public int ErrCode { get; set; }

    public string ErrMsg { get; set; } = string.Empty;
}

