namespace RiskAnalyzer.Api.Config;

/// <summary>
/// Configuration settings for Google Gemini AI API
/// Binds to "GeminiSettings" section in appsettings.json
/// </summary>
public class GeminiSettings
{
    /// <summary>Google Gemini API Key</summary>
    public string ApiKey { get; set; } = null!;
    
    /// <summary>Gemini model ID to use for image analysis</summary>
    public string ModelId { get; set; } = "gemini-2.0-flash";
    
    /// <summary>Maximum retry attempts for a single image analysis</summary>
    public int MaxAttemptsPerImage { get; set; } = 3;
    
    /// <summary>API request timeout in seconds</summary>
    public int TimeoutSeconds { get; set; } = 30;
}
