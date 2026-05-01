namespace RiskAnalyzer.Api.Config;

/// <summary>
/// Configuration settings for ML.NET fine-tuning
/// Binds to "MLSettings" section in appsettings.json
/// </summary>
public class MLSettings
{
    /// <summary>Path to trained model file</summary>
    public string ModelPath { get; set; } = "ml_models/image_classifier.zip";

    /// <summary>Path to training data</summary>
    public string TrainingDataPath { get; set; } = "TrainingData";

    /// <summary>Minimum confidence threshold for predictions (0-1)</summary>
    public float ConfidenceThreshold { get; set; } = 0.7f;

    /// <summary>Auto-block risk score threshold</summary>
    public float AutoBlockThreshold { get; set; } = 8.0f;

    /// <summary>Review queue risk score threshold</summary>
    public float ReviewThreshold { get; set; } = 5.0f;

    /// <summary>Allow upload risk score threshold</summary>
    public float AllowThreshold { get; set; } = 5.0f;

    /// <summary>Enable model training on startup</summary>
    public bool TrainOnStartup { get; set; } = false;

    /// <summary>Number of training epochs</summary>
    public int Epochs { get; set; } = 100;

    /// <summary>Batch size for training</summary>
    public int BatchSize { get; set; } = 32;
}
