namespace RiskAnalyzer.Api.Models;

/// <summary>
/// Represents a tag/label associated with an image analysis
/// Examples: "nudity", "violence", "weapons", "safe", etc.
/// </summary>
public class AnalysisTag
{
    /// <summary>Primary key - Tag ID (GUID)</summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>Foreign key - Analysis record this tag belongs to</summary>
    public Guid ImageAnalysisId { get; set; }
    
    /// <summary>Navigation property - The analysis record</summary>
    public ImageAnalysis ImageAnalysis { get; set; } = null!;
    
    /// <summary>Tag name (e.g., "nudity", "violence", "weapons")</summary>
    public string Name { get; set; } = null!;
    
    /// <summary>Confidence score for this tag (0-100)</summary>
    public int Confidence { get; set; }
    
    /// <summary>Category of the tag (e.g., "safety", "content", "other")</summary>
    public string Category { get; set; } = null!;
}
