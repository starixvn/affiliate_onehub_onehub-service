using System.Text.Json.Serialization;

namespace ONEHub.CronJob.Models;

/// <summary>
/// Response model for token authentication
/// </summary>
public class TokenResponse
{
    /// <summary>
    /// JWT access token
    /// </summary>
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = null!;

    /// <summary>
    /// Token type (e.g., "JwtSecurityToken")
    /// </summary>
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = null!;

    /// <summary>
    /// Token expiration time in minutes
    /// </summary>
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
}

