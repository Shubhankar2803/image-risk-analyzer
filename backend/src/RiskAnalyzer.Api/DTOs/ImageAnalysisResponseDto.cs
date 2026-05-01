namespace RiskAnalyzer.Api.DTOs;

public class ImageAnalysisResponseDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = null!;
    public long FileSizeBytes { get; set; }
    public float RiskScore { get; set; }
    public string Classification { get; set; } = null!;
    public string AnalysisDetails { get; set; } = null!;
    public float ConfidenceScore { get; set; }
    public int SeverityWeight { get; set; }
    public string RiskAction { get; set; } = "Review";
    public string RiskColor { get; set; } = "yellow";
    public DateTime AnalyzedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public ICollection<AnalysisTagDto> Tags { get; set; } = new List<AnalysisTagDto>();
    public string? Categories { get; set; }
}
