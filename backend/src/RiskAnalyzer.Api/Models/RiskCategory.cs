namespace RiskAnalyzer.Api.Models;

/// <summary>
/// Risk category with severity weight and action threshold
/// Stored in database to allow dynamic configuration without recompilation
/// </summary>
public class RiskCategory
{
    public int Id { get; set; }
    
    /// <summary>Category name matching folder structure (e.g., "02_Violence_Gore")</summary>
    public string CategoryName { get; set; } = null!;
    
    /// <summary>Severity weight on 0-10 scale (0=safe, 10=critical)</summary>
    public int SeverityWeight { get; set; }
    
    /// <summary>Action threshold - confidence level at which to trigger action (0-1)</summary>
    public float ActionThreshold { get; set; }
    
    /// <summary>Recommended action: Auto-Block, Review, Allow, Warning</summary>
    public string RecommendedAction { get; set; } = "Review";
    
    /// <summary>Human-readable description</summary>
    public string Description { get; set; } = null!;
    
    /// <summary>Whether this category is enabled for filtering</summary>
    public bool IsEnabled { get; set; } = true;
    
    /// <summary>Last updated timestamp</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
