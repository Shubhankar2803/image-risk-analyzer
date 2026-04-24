namespace RiskAnalyzer.Api.DTOs;

public class ImageAnalysisDetailDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = null!;
    public long FileSizeBytes { get; set; }
    public int RiskScore { get; set; }
    public string Classification { get; set; } = null!;
    public string AnalysisDetails { get; set; } = null!;
    public int ConfidenceScore { get; set; }
    public DateTime AnalyzedAt { get; set; }
    public List<AnalysisTagDto> Tags { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class BatchAnalysisRequest
{
    public List<string> FileIds { get; set; } = new();
}

public class BatchAnalysisResponse
{
    public int TotalCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    
    /// <summary>List of analysis results</summary>
    public List<ImageAnalysisResponseDto> Results { get; set; } = new();
}
