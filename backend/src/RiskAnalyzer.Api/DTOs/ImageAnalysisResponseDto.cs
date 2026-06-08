namespace RiskAnalyzer.Api.DTOs;

public class ImageAnalysisResponseDto
{
    public Guid ImageId { get; set; }
    public string FileName { get; set; } = null!;
    public string Status { get; set; } = "Completed";
    public ImageAnalysisResultDto AnalysisResult { get; set; } = new();
    
    // Legacy fields for backward compatibility
    public Guid Id { get; set; }
    public long FileSizeBytes { get; set; }
    public decimal RiskScore { get; set; }
    public string Classification { get; set; } = null!;
    public string AnalysisDetails { get; set; } = null!;
    public decimal ConfidenceScore { get; set; }
    public DateTime AnalyzedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Categories { get; set; }
}

public class ImageAnalysisResultDto
{
    /// <summary>Overall risk score for the image (0-1 or 0-100)</summary>
    public decimal OverallRiskScore { get; set; }
    
    /// <summary>List of detected risk tags</summary>
    public List<AnalysisTagDto> Tags { get; set; } = new();
}
