using Microsoft.ML;
using Microsoft.ML.Vision;
using RiskAnalyzer.Api.Config;
using RiskAnalyzer.Api.ML.Models;

namespace RiskAnalyzer.Api.Services;

/// <summary>
/// Service for ML.NET image classification model operations.
/// Handles model training, loading, and inference for risk analysis.
/// </summary>
public interface IMLModelService
{
    Task<ImagePrediction> PredictImageAsync(string imagePath);
    Task<ImagePrediction> PredictImageAsync(byte[] imageBytes);
    Task TrainModelAsync();
    bool IsModelLoaded { get; }
}

public class MLModelService : IMLModelService
{
    private readonly ILogger<MLModelService> _logger;
    private readonly MLContext _mlContext;
    private ITransformer? _model;
    private PredictionEngine<ImageInputData, ImageOutputData>? _predictionEngine;
    private readonly MLSettings _mlSettings;
    private readonly string _basePath;
    private bool _isInitialized = false;

    public bool IsModelLoaded => _model != null && _isInitialized;

    public MLModelService(ILogger<MLModelService> logger, MLSettings mlSettings)
    {
        _logger = logger;
        _mlSettings = mlSettings;
        _mlContext = new MLContext(seed: 1);
        _basePath = Directory.GetCurrentDirectory();
    }

    /// <summary>
    /// Initialize model service - loads trained model or trains if not exists
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            var modelPath = Path.Combine(_basePath, _mlSettings.ModelPath);

            if (File.Exists(modelPath))
            {
                _logger.LogInformation("Loading existing model from {ModelPath}", modelPath);
                await LoadModelAsync(modelPath);
            }
            else if (_mlSettings.TrainOnStartup)
            {
                _logger.LogInformation("Training model on startup");
                await TrainModelAsync();
            }
            else
            {
                _logger.LogWarning("Model not found at {ModelPath} and TrainOnStartup is disabled", modelPath);
                _logger.LogWarning("Call TrainModelAsync() manually to train the model");
            }

            _isInitialized = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing ML model");
            _isInitialized = false;
        }
    }

    /// <summary>
    /// Train model using images from training data directory.
    /// Directory structure: TrainingData/[Label]/image.jpg
    /// </summary>
    public async Task TrainModelAsync()
    {
        try
        {
            var trainingDataPath = Path.Combine(_basePath, _mlSettings.TrainingDataPath);

            if (!Directory.Exists(trainingDataPath))
            {
                _logger.LogError("Training data path not found: {TrainingDataPath}", trainingDataPath);
                throw new DirectoryNotFoundException($"Training data directory not found: {trainingDataPath}");
            }

            _logger.LogInformation("Starting model training from {TrainingDataPath}", trainingDataPath);

            // Load training images
            var imageData = LoadImagesFromDirectory(trainingDataPath);
            if (imageData.Count == 0)
            {
                _logger.LogError("No training images found in {TrainingDataPath}", trainingDataPath);
                throw new InvalidOperationException("No training images found");
            }

            var trainingData = _mlContext.Data.LoadFromEnumerable(imageData);

            // Create pipeline for image classification
            var pipeline = _mlContext.Transforms.ResizeImages(
                    outputColumnName: "input",
                    imageWidth: 224,
                    imageHeight: 224,
                    inputColumnName: "ImagePath")
                .Append(_mlContext.Transforms.ExtractPixels(
                    outputColumnName: "input",
                    inputColumnName: "input",
                    interleavePixelColors: true))
                .Append(_mlContext.MulticlassClassification.Trainers.ImageClassification(
                    labelColumnName: "Label",
                    featureColumnName: "input",
                    validationSet: null,
                    testOnTrainSet: true));

            _logger.LogInformation("Training image classification model with {ImageCount} images", imageData.Count);

            _model = pipeline.Fit(trainingData);

            // Save model
            var modelPath = Path.Combine(_basePath, _mlSettings.ModelPath);
            Directory.CreateDirectory(Path.GetDirectoryName(modelPath)!);
            _mlContext.Model.Save(_model, trainingData.Schema, modelPath);

            _logger.LogInformation("Model training completed. Saved to {ModelPath}", modelPath);

            // Create prediction engine
            _predictionEngine = _mlContext.Model.CreatePredictionEngine<ImageInputData, ImageOutputData>(_model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error training ML model");
            throw;
        }
    }

    /// <summary>
    /// Predict image classification and calculate risk score
    /// </summary>
    public async Task<ImagePrediction> PredictImageAsync(string imagePath)
    {
        return await Task.Run(() =>
        {
            try
            {
                if (!IsModelLoaded)
                {
                    throw new InvalidOperationException("Model is not loaded. Initialize the service first.");
                }

                if (!File.Exists(imagePath))
                {
                    throw new FileNotFoundException($"Image file not found: {imagePath}");
                }

                var input = new ImageInputData { ImagePath = imagePath };
                var prediction = _predictionEngine!.Predict(input);

                return MapToPrediction(prediction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error predicting image");
                throw;
            }
        });
    }

    /// <summary>
    /// Predict image from byte array
    /// </summary>
    public async Task<ImagePrediction> PredictImageAsync(byte[] imageBytes)
    {
        try
        {
            // Save bytes to temporary file
            var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.jpg");
            await File.WriteAllBytesAsync(tempPath, imageBytes);

            try
            {
                return await PredictImageAsync(tempPath);
            }
            finally
            {
                // Clean up temp file
                if (File.Exists(tempPath))
                    File.Delete(tempPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error predicting image from bytes");
            throw;
        }
    }

    private async Task LoadModelAsync(string modelPath)
    {
        try
        {
            using (var stream = new FileStream(modelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                _model = _mlContext.Model.Load(stream, out _);
            }

            _predictionEngine = _mlContext.Model.CreatePredictionEngine<ImageInputData, ImageOutputData>(_model);
            _logger.LogInformation("Model loaded successfully from {ModelPath}", modelPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading model from {ModelPath}", modelPath);
            throw;
        }
    }

    private List<ImageInputData> LoadImagesFromDirectory(string imagesPath)
    {
        var imageData = new List<ImageInputData>();

        try
        {
            var supportedExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };
            var directories = Directory.GetDirectories(imagesPath);

            foreach (var directory in directories)
            {
                var label = Path.GetFileName(directory);
                var files = Directory.GetFiles(directory);

                foreach (var file in files)
                {
                    var ext = Path.GetExtension(file).ToLower();
                    if (supportedExtensions.Contains(ext))
                    {
                        imageData.Add(new ImageInputData
                        {
                            ImagePath = file,
                            Label = label
                        });
                    }
                }

                _logger.LogInformation("Loaded {ImageCount} images for label '{Label}'", 
                    files.Length, label);
            }

            return imageData;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading images from directory");
            throw;
        }
    }

    private ImagePrediction MapToPrediction(ImageOutputData prediction)
    {
        var category = prediction.Prediction;
        var confidenceScore = prediction.Score.Length > 0 ? prediction.Score[0] : 0f;

        // Clamp confidence to 0-1 range
        confidenceScore = Math.Max(0f, Math.Min(1f, confidenceScore));

        // Return raw prediction - caller will use RiskScoringService for final scoring
        return new ImagePrediction
        {
            Category = category,
            Confidence = confidenceScore,
            FinalRiskScore = 0,  // Will be calculated by RiskScoringService
            Action = "Pending",
            SeverityWeight = 0,  // Will be fetched from database
            RiskColor = "gray"   // Will be determined by RiskScoringService
        };
    }
}

/// <summary>
/// Represents a prediction result from the ML model
/// </summary>
public class ImagePrediction
{
    public string Category { get; set; } = null!;
    public float Confidence { get; set; }
    public float FinalRiskScore { get; set; }
    public int SeverityWeight { get; set; }
    public string Action { get; set; } = null!;
    public string RiskColor { get; set; } = null!;
}
