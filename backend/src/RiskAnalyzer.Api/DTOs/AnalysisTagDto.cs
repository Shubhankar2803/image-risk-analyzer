namespace RiskAnalyzer.Api.DTOs;

/// <summary>
/// DTO for analysis tags
/// </summary>
public class AnalysisTagDto
{
    /// <summary>Tag category (e.g., "Privacy & PII", "Safety & Violence")</summary>
    public string Category { get; set; } = null!;
    
    /// <summary>Tag name</summary>
    public string Name { get; set; } = null!;
    
    /// <summary>Confidence score for this tag (0-1 scale)</summary>
    public decimal Confidence { get; set; }
    
    /// <summary>Severity level: High, Medium, or Low</summary>
    public string Severity { get; set; } = "Low";
}
