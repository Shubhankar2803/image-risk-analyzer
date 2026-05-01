using Microsoft.ML.Data;

namespace RiskAnalyzer.Api.ML.Models;

/// <summary>
/// Output data from image classification model.
/// Contains predictions and confidence scores.
/// </summary>
public class ImageOutputData
{
    [ColumnName("PredictedLabel")]
    public string Prediction { get; set; } = null!;

    public float[] Score { get; set; } = null!;
}
