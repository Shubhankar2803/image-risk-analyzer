namespace RiskAnalyzer.Api.DTOs;

/// <summary>
/// DTO for analysis tags
/// </summary>
public class AnalysisTagDto
{
    /// <summary>Tag name</summary>
    public string Name { get; set; } = null!;
    
    /// <summary>Confidence score for this tag (0-100)</summary>
    public int Confidence { get; set; }
    
    /// <summary>Tag category (e.g., "safety", "content")</summary>
    public string Category { get; set; } = null!;
}
