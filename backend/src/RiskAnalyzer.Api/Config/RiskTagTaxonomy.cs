namespace RiskAnalyzer.Api.Config;

/// <summary>
/// Defines the taxonomy of risk tags and their severity levels
/// </summary>
public class RiskTagTaxonomy
{
    public class RiskTag
    {
        public string Name { get; set; } = null!;
        public string Category { get; set; } = null!;
        public string Severity { get; set; } = null!; // High, Medium, Low
        public string Description { get; set; } = null!;
        public decimal ConfidenceThreshold { get; set; } // Minimum confidence to flag
    }

    /// <summary>
    /// All predefined risk tags in the system
    /// </summary>
    public static readonly List<RiskTag> PredefinedTags = new()
    {
        // Privacy & PII Tags
        new RiskTag
        {
            Name = "Financial Information",
            Category = "Privacy & PII",
            Severity = "High",
            Description = "Credit cards, bank statements, or checks.",
            ConfidenceThreshold = 0.85m
        },
        new RiskTag
        {
            Name = "Identification Documents",
            Category = "Privacy & PII",
            Severity = "High",
            Description = "Driver's licenses, passports, Social Security cards.",
            ConfidenceThreshold = 0.85m
        },
        new RiskTag
        {
            Name = "Credit Card Number",
            Category = "Privacy & PII",
            Severity = "High",
            Description = "Visible credit card numbers or financial account information.",
            ConfidenceThreshold = 0.90m
        },
        new RiskTag
        {
            Name = "Physical Address",
            Category = "Privacy & PII",
            Severity = "Medium",
            Description = "Visible residential or business addresses.",
            ConfidenceThreshold = 0.80m
        },
        new RiskTag
        {
            Name = "Face Detected",
            Category = "Privacy & PII",
            Severity = "Medium",
            Description = "Clear faces of individuals (useful for platforms requiring consent or child safety).",
            ConfidenceThreshold = 0.85m
        },
        new RiskTag
        {
            Name = "License Plates",
            Category = "Vehicle Information",
            Severity = "Medium",
            Description = "Vehicle registration plates (may need auto-blurring rather than blocking).",
            ConfidenceThreshold = 0.80m
        },
        new RiskTag
        {
            Name = "Unconsented Faces",
            Category = "Privacy & PII",
            Severity = "Medium",
            Description = "Clear faces of individuals requiring consent.",
            ConfidenceThreshold = 0.85m
        },

        // Safety Tags
        new RiskTag
        {
            Name = "Violence",
            Category = "Safety & Violence",
            Severity = "High",
            Description = "Images containing violent content or physical harm.",
            ConfidenceThreshold = 0.85m
        },
        new RiskTag
        {
            Name = "Weapons",
            Category = "Safety & Violence",
            Severity = "High",
            Description = "Visible weapons or dangerous objects.",
            ConfidenceThreshold = 0.80m
        },
        new RiskTag
        {
            Name = "Harassment & Hate Speech",
            Category = "Harmful Content",
            Severity = "High",
            Description = "Content promoting harassment or hate speech.",
            ConfidenceThreshold = 0.85m
        },
        new RiskTag
        {
            Name = "Sexually Explicit Content",
            Category = "Adult Content",
            Severity = "High",
            Description = "Sexually explicit or adult content.",
            ConfidenceThreshold = 0.90m
        }
    };

    /// <summary>
    /// Get a risk tag by name
    /// </summary>
    public static RiskTag? GetTagByName(string name) =>
        PredefinedTags.FirstOrDefault(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Get all tags by category
    /// </summary>
    public static List<RiskTag> GetTagsByCategory(string category) =>
        PredefinedTags.Where(t => t.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();

    /// <summary>
    /// Get all tags by severity level
    /// </summary>
    public static List<RiskTag> GetTagsBySeverity(string severity) =>
        PredefinedTags.Where(t => t.Severity.Equals(severity, StringComparison.OrdinalIgnoreCase)).ToList();
}
