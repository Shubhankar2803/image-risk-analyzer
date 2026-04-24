namespace RiskAnalyzer.Api.Config;

/// <summary>
/// Configuration settings for CORS policy
/// Binds to "Cors" section in appsettings.json
/// </summary>
public class CorsSettings
{
    /// <summary>Comma-separated list of allowed origins (e.g., "http://localhost:4200,http://localhost:3000")</summary>
    public string AllowedOrigins { get; set; } = "http://localhost:4200,http://localhost:3000";
    
    /// <summary>Comma-separated list of allowed HTTP methods</summary>
    public string AllowedMethods { get; set; } = "GET,POST,PUT,DELETE,OPTIONS";
    
    /// <summary>Comma-separated list of allowed headers</summary>
    public string AllowedHeaders { get; set; } = "Content-Type,Authorization";
}
