namespace RiskAnalyzer.Api.ML;

/// <summary>
/// DEPRECATED: Use RiskCategoryService instead for database-driven severity weights
/// This class is kept for reference but all values should be fetched from database
/// </summary>
public static class CategorySeverity
{
    /// <summary>
    /// Mapping of category labels to severity weights (0-10 scale)
    /// DEPRECATED: These are default values only. Use database for actual values.
    /// </summary>
    public static readonly Dictionary<string, (int SeverityWeight, string Action, string Description)> Categories = new()
    {
        // Safe content - no action needed
        { "Safe", (0, "No Action", "Control group - safe content") },

        // Critical risks - auto-block (weight 10)
        { "Explicit_Porn", (10, "Auto-Block", "High legal/policy risk") },
        { "Violence_gore", (10, "Auto-Block", "Triggers 'Shock' or 'Harm' policies") },
        { "Hate_Symbols", (10, "Auto-Block", "High risk of platform de-platforming") },

        // High risk - requires review/help (weight 9)
        { "Softporn", (9, "Block + Review", "Sexually suggestive content") },

        // Moderate risk - requires review (weight 7)
        { "Weapons", (7, "Review", "Could be legitimate (movie poster) or threat") },

        // Low-moderate risk - sensitive content (weight 4-6)
        { "Hentai", (6, "Review", "Animated explicit content - platform dependent") },
        { "Sensitive_Documents", (6, "Review", "May contain PII or confidential info") },

        // Minimal risk - warning only (weight 2-4)
        { "VIolence_gore", (4, "Warning", "Realistic but non-graphic violence") }
    };

    /// <summary>
    /// Calculate final risk score: AI Confidence × Severity Weight × 10
    /// Returns value on 0-100 scale
    /// Formula: Final Risk = Confidence (0-1) × SeverityWeight (0-10) × 10
    /// </summary>
    public static float CalculateFinalRiskScore(float confidence, int severityWeight)
    {
        // Clamp confidence to 0-1 range
        confidence = Math.Max(0f, Math.Min(1f, confidence));
        // Calculate: confidence (0-1) × weight (0-10) × 10 = score (0-100)
        return (confidence * severityWeight * 10f);
    }

    /// <summary>
    /// Determine action based on final risk score
    /// Score > 8.0: Instant Delete (Auto-Block)
    /// Score 5.0 – 8.0: Admin Review queue
    /// Score < 5.0: Allow upload
    /// </summary>
    public static string DetermineAction(float finalRiskScore)
    {
        return finalRiskScore switch
        {
            > 8.0f => "Auto-Block",
            >= 5.0f and <= 8.0f => "Review",
            < 5.0f => "Allow",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Get color code for UI display
    /// 0–3: Green (Safe)
    /// 4–6: Yellow (Warning)
    /// 7–10: Red (Danger)
    /// </summary>
    public static string GetRiskColor(float riskScore)
    {
        return riskScore switch
        {
            <= 3f => "green",
            <= 6f => "yellow",
            _ => "red"
        };
    }
}
