using Fanex.Data;
using Fanex.Data.Repository;

namespace ONEHub.CronJob.Repositories.Criterias;

/// <summary>
/// Criteria for getting last imported time for member data
/// </summary>
public class GetMemberInfoImportedTimeCriteria : ICriteria
{
    public string GetSettingKey() => "Task_OH_GetSetting";

    public bool IsValid() => !string.IsNullOrEmpty(DataSource);

    /// <summary>
    /// Data source identifier (e.g., "MemberInfoLastImportedTime")
    /// Maps to stored procedure parameter
    /// </summary>
    [SpParam(SqlParamName = "SName")]
    public string DataSource { get; set; } = null!;
}

