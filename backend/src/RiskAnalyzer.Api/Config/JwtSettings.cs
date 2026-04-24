namespace RiskAnalyzer.Api.Config;

/// <summary>
/// Configuration settings for JWT authentication
/// Binds to "JwtSettings" section in appsettings.json
/// </summary>
public class JwtSettings
{
    /// <summary>Secret key for signing JWT tokens (minimum 32 characters)</summary>
    public string SecretKey { get; set; } = null!;
    
    /// <summary>Token expiration time in minutes</summary>
    public int ExpirationMinutes { get; set; } = 60;
    
    /// <summary>JWT issuer claim</summary>
    public string Issuer { get; set; } = null!;
    
    /// <summary>JWT audience claim</summary>
    public string Audience { get; set; } = null!;
}
