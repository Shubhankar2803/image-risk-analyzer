namespace RiskAnalyzer.Api.DTOs;

public class ImageAnalysisResponseDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = null!;
    public long FileSizeBytes { get; set; }
    public int RiskScore { get; set; }
    public string Classification { get; set; } = null!;
    public string AnalysisDetails { get; set; } = null!;
    public int ConfidenceScore { get; set; }
    public DateTime AnalyzedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<AnalysisTagDto> Tags { get; set; } = new List<AnalysisTagDto>();
    public string? Categories { get; set; }
}
