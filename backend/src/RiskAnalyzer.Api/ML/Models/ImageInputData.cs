using Microsoft.ML.Data;

namespace RiskAnalyzer.Api.ML.Models;

/// <summary>
/// Input data for image classification model.
/// Expects image path for training/inference.
/// </summary>
public class ImageInputData
{
    [LoadColumn(0)]
    public string ImagePath { get; set; } = null!;

    [LoadColumn(1)]
    public string Label { get; set; } = null!;
}
