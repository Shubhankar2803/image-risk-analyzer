namespace RiskAnalyzer.Api.Models;

public class ImageAnalysis
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public string FilePath { get; set; } = null!;
    public long FileSizeBytes { get; set; }
    public string ContentType { get; set; } = null!;
    public int RiskScore { get; set; }
    public string Classification { get; set; } = null!;
    public string AnalysisDetails { get; set; } = null!;
    public int ConfidenceScore { get; set; }
    public DateTime AnalyzedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? Categories { get; set; }
    public ICollection<AnalysisTag> Tags { get; set; } = new List<AnalysisTag>();
}
