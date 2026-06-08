namespace RiskAnalyzer.Api.Models;

using System.ComponentModel.DataAnnotations;

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
    [MaxLength(100)]
    public string Name { get; set; } = null!;
    
    /// <summary>Confidence score for this tag (0-100)</summary>
    public int Confidence { get; set; }
    
    /// <summary>Category of the tag (e.g., "safety", "content", "other")</summary>
    [MaxLength(50)]
    public string Category { get; set; } = null!;
    
    /// <summary>Severity level of the risk (High, Medium, Low)</summary>
    [MaxLength(50)]
    public string Severity { get; set; } = "Low";
}
