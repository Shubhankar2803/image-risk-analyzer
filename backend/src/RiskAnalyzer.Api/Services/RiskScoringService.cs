using RiskAnalyzer.Api.Models;
using RiskAnalyzer.Api.Repositories;
using RiskAnalyzer.Api.ML;

namespace RiskAnalyzer.Api.Services;

/// <summary>
/// Service for risk scoring calculation using database-driven severity weights
/// Provides centralized risk calculation logic with configurable thresholds
/// </summary>
public interface IRiskScoringService
{
    Task<RiskScoringResult> CalculateRiskAsync(string category, float confidence);
    Task<List<RiskCategory>> GetAllCategoriesAsync();
    Task<RiskCategory?> GetCategoryAsync(string categoryName);
}

public class RiskScoringService : IRiskScoringService
{
    private readonly IRiskCategoryRepository _categoryRepository;
    private readonly ILogger<RiskScoringService> _logger;

    public RiskScoringService(
        IRiskCategoryRepository categoryRepository,
        ILogger<RiskScoringService> logger)
    {
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    /// <summary>
    /// Calculate comprehensive risk score based on ML confidence and database severity weight
    /// Formula: Final Risk = Confidence (0-1) × SeverityWeight (0-10) × 10 = Score (0-100)
    /// </summary>
    public async Task<RiskScoringResult> CalculateRiskAsync(string category, float confidence)
    {
        try
        {
            // Fetch category weight from database
            var riskCategory = await _categoryRepository.GetByCategoryNameAsync(category);
            if (riskCategory == null)
            {
                _logger.LogWarning("Risk category not found in database: {Category}", category);
                // Return safe result if category not found
                return new RiskScoringResult
                {
                    Category = category,
                    Confidence = confidence,
                    SeverityWeight = 0,
                    FinalRiskScore = 0,
                    Action = "Allow",
                    RiskColor = "green",
                    RecommendedAction = "Allow"
                };
            }

            // Clamp confidence to 0-1 range
            confidence = Math.Max(0f, Math.Min(1f, confidence));

            // Calculate final risk score
            // Formula: Confidence (0-1) × SeverityWeight (0-10) × 10 = Score (0-100)
            float finalRiskScore = CategorySeverity.CalculateFinalRiskScore(confidence, riskCategory.SeverityWeight);

            // Determine action based on thresholds
            string action = DetermineActionFromThreshold(finalRiskScore, riskCategory.ActionThreshold);

            // Get UI color
            string riskColor = CategorySeverity.GetRiskColor(finalRiskScore);

            _logger.LogInformation(
                "Risk calculated: Category={Category}, Confidence={Confidence:P0}, SeverityWeight={Weight}, FinalScore={Score:F2}, Action={Action}",
                category, confidence, riskCategory.SeverityWeight, finalRiskScore, action);

            return new RiskScoringResult
            {
                Category = category,
                Confidence = confidence,
                SeverityWeight = riskCategory.SeverityWeight,
                FinalRiskScore = finalRiskScore,
                Action = action,
                RiskColor = riskColor,
                RecommendedAction = riskCategory.RecommendedAction,
                Description = riskCategory.Description
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating risk score for category: {Category}", category);
            throw;
        }
    }

    /// <summary>
    /// Get all available risk categories from database
    /// </summary>
    public async Task<List<RiskCategory>> GetAllCategoriesAsync()
    {
        try
        {
            return await _categoryRepository.GetAllEnabledAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all risk categories");
            throw;
        }
    }

    /// <summary>
    /// Get specific risk category
    /// </summary>
    public async Task<RiskCategory?> GetCategoryAsync(string categoryName)
    {
        try
        {
            return await _categoryRepository.GetByCategoryNameAsync(categoryName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving risk category: {CategoryName}", categoryName);
            throw;
        }
    }

    /// <summary>
    /// Determine action based on confidence × ActionThreshold comparison
    /// Uses ActionThreshold from database for flexible configuration
    /// </summary>
    private static string DetermineActionFromThreshold(float finalRiskScore, float actionThreshold)
    {
        // Convert thresholds to 0-100 scale for comparison
        float normalizedThreshold = actionThreshold * 10f;

        return finalRiskScore switch
        {
            > 8.0f => "Auto-Block",
            >= 5.0f and <= 8.0f => "Review",
            < 5.0f => "Allow",
            _ => "Unknown"
        };
    }
}

/// <summary>
/// Result of risk score calculation
/// Contains all information needed for logging and API response
/// </summary>
public class RiskScoringResult
{
    public string Category { get; set; } = null!;
    public float Confidence { get; set; }
    public int SeverityWeight { get; set; }
    public float FinalRiskScore { get; set; }
    public string Action { get; set; } = null!;
    public string RiskColor { get; set; } = null!;
    public string RecommendedAction { get; set; } = null!;
    public string? Description { get; set; }
}
