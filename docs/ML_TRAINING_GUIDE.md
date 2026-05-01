# ML.NET Fine-Tuned Model Training Guide

This guide explains how to prepare training data and train the ML.NET image classification model for the Image Risk Analyzer.

## Training Data Structure

The model expects training data organized by category labels in the following directory structure:

```
TrainingData/
├── Safe/
│   ├── image1.jpg
│   ├── image2.png
│   └── ...
├── Explicit_Porn/
│   ├── image1.jpg
│   ├── image2.png
│   └── ...
├── Violence_gore/
│   ├── image1.jpg
│   └── ...
├── Hate_Symbols/
│   ├── image1.jpg
│   └── ...
├── Softporn/
│   ├── image1.jpg
│   └── ...
├── Weapons/
│   ├── image1.jpg
│   └── ...
├── Hentai/
│   ├── image1.jpg
│   └── ...
├── Sensitive_Documents/
│   ├── image1.jpg
│   └── ...
└── VIolence_gore/
    ├── image1.jpg
    └── ...
```

## Category Labels and Severity Weights

| Category | Severity Weight | Action | Description |
|----------|-----------------|--------|-------------|
| Safe | 0 | No Action | Control group - safe content |
| Explicit_Porn | 10 | Auto-Block | High legal/policy risk |
| Violence_gore | 10 | Auto-Block | Triggers harm policies |
| Hate_Symbols | 10 | Auto-Block | Platform de-platforming risk |
| Softporn | 9 | Block + Review | Sexually suggestive |
| Weapons | 7 | Review | Could be movie poster or threat |
| Hentai | 6 | Review | Animated explicit content |
| Sensitive_Documents | 6 | Review | May contain PII |
| VIolence_gore | 4 | Warning | Non-graphic violence |

## Risk Scoring Formula

```
Final Risk Score = AI Confidence × Severity Weight × 10
```

**Example:**
- Confidence: 92% (0.92)
- Severity Weight: 7 (Weapons)
- Final Risk Score: 0.92 × 7 × 10 = **64.4**

## Risk Action Thresholds

- **Score > 8.0**: Auto-Block (instant deletion)
- **Score 5.0 - 8.0**: Review (admin review queue)
- **Score < 5.0**: Allow (upload permitted)

## UI Color Coding

- **0–3 (Green)**: Safe
- **4–6 (Yellow)**: Warning
- **7–10 (Red)**: Danger

## Training Configuration

Edit `appsettings.Development.json`:

```json
{
  "MLSettings": {
    "ModelPath": "ml_models/image_classifier.zip",
    "TrainingDataPath": "TrainingData",
    "ConfidenceThreshold": 0.7,
    "AutoBlockThreshold": 8.0,
    "ReviewThreshold": 5.0,
    "AllowThreshold": 5.0,
    "TrainOnStartup": false,
    "Epochs": 100,
    "BatchSize": 32
  }
}
```

### Configuration Parameters

- **ModelPath**: Where to save/load the trained model
- **TrainingDataPath**: Path to training data directory
- **ConfidenceThreshold**: Minimum confidence for predictions (0-1)
- **TrainOnStartup**: Automatically train model when app starts
- **Epochs**: Number of training iterations (higher = more training)
- **BatchSize**: Number of images to process per training batch

## Training Process

### Option 1: Manual Training

1. **Prepare Training Data**
   ```
   Create TrainingData/ directory with subdirectories for each category
   Add at least 50-100 images per category for good results
   ```

2. **Set TrainOnStartup = true** (optional)
   ```json
   "TrainOnStartup": true
   ```

3. **Start the Application**
   ```bash
   cd backend/src/RiskAnalyzer.Api
   dotnet restore
   dotnet build
   dotnet run
   ```

4. **Monitor Training**
   - Check logs for training progress
   - Model will be saved to `ml_models/image_classifier.zip`

### Option 2: Programmatic Training

Create an API endpoint for training:

```csharp
[HttpPost("train")]
[Authorize]
public async Task<IActionResult> TrainModel()
{
    try
    {
        await _mlModelService.TrainModelAsync();
        return Ok(new { message = "Model training started" });
    }
    catch (Exception ex)
    {
        return BadRequest(new { error = ex.Message });
    }
}
```

## Image Requirements

- **Formats**: JPG, JPEG, PNG, BMP, GIF
- **Size**: Minimum 224×224 pixels (recommended)
- **Quality**: Clear, representative images for each category
- **Quantity**: Minimum 50-100 images per category (500+ recommended)
- **Balance**: Similar number of images per category for best results

## Best Practices

1. **Data Quality**
   - Use clear, representative images
   - Avoid blurry or low-quality images
   - Ensure labels are accurate

2. **Data Balance**
   - Try to have similar number of images per category
   - If imbalanced, use more epochs to train longer

3. **Model Evaluation**
   - Test with unknown images before production
   - Monitor prediction accuracy and false positives
   - Retrain with new data if accuracy drops

4. **Production Deployment**
   - Train locally first
   - Validate accuracy on test set
   - Deploy trained model (`ml_models/image_classifier.zip`)
   - Keep original training data for retraining

## Troubleshooting

### Model Not Loading

```csharp
// Check if model file exists
if (!File.Exists("ml_models/image_classifier.zip"))
{
    // Train the model first
    await _mlModelService.TrainModelAsync();
}
```

### Low Accuracy

- Add more training images (>500 per category)
- Increase Epochs (try 200-300)
- Ensure data is balanced across categories
- Use higher quality images

### Out of Memory

- Reduce BatchSize (try 16 or 8)
- Process images in smaller batches
- Use a machine with more RAM for training

### Slow Training

- Reduce number of Epochs
- Reduce image resolution
- Use a GPU-enabled machine (if available)

## Next Steps

1. Create `TrainingData/` directory structure
2. Add representative images to each category
3. Configure `MLSettings` in `appsettings.Development.json`
4. Train the model
5. Test predictions with sample images
6. Deploy to production when satisfied

## References

- [ML.NET Documentation](https://learn.microsoft.com/en-us/dotnet/machine-learning/)
- [Image Classification](https://learn.microsoft.com/en-us/dotnet/machine-learning/tutorials/image-classification)
- [Transfer Learning](https://learn.microsoft.com/en-us/dotnet/machine-learning/tutorials/image-classification-api-transfer-learning)
