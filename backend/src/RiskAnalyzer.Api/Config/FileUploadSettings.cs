namespace RiskAnalyzer.Api.Config;

/// <summary>
/// Configuration settings for file upload handling
/// Binds to "FileUpload" section in appsettings.json
/// </summary>
public class FileUploadSettings
{
    /// <summary>Maximum file size in bytes (default: 5MB)</summary>
    public long MaxFileSizeBytes { get; set; } = 5242880;
    
    /// <summary>Comma-separated list of allowed file extensions (e.g., ".jpg,.png")</summary>
    public string AllowedExtensions { get; set; } = ".jpg,.jpeg,.png,.webp,.gif";
    
    /// <summary>Local directory path where uploaded files are stored</summary>
    public string StoragePath { get; set; } = "uploads";
}
