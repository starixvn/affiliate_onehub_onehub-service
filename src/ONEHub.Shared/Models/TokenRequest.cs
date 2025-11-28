using System.ComponentModel.DataAnnotations;

namespace ONEHub.Shared.Models;

/// <summary>
/// Configuration for token authentication request
/// </summary>
public class TokenRequest
{
    /// <summary>
    /// Username for authentication
    /// </summary>
    [Required(ErrorMessage = "UserName is required")]
    public string UserName { get; set; } = null!;

    /// <summary>
    /// Password for authentication
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = null!;

    /// <summary>
    /// Token cache timeout in minutes
    /// </summary>
    [Range(1, 1440, ErrorMessage = "CacheTimeOut must be between 1 and 1440 minutes")]
    public int CacheTimeOut { get; set; } = 20;
}

