# Training Data Directory

This directory contains the training data for the ML.NET image classification model.

## Structure

Create subdirectories for each content category with images:

```
TrainingData/
├── Safe/                    # Safe content images
├── Explicit_Porn/           # Explicit pornographic content
├── Violence_gore/           # Violent and gory content
├── Hate_Symbols/            # Hate symbols and extremist content
├── Softporn/                # Sexually suggestive content
├── Weapons/                 # Images containing weapons
├── Hentai/                  # Animated explicit content
├── Sensitive_Documents/     # Documents with sensitive information
└── VIolence_gore/           # Non-graphic violence
```

## Guidelines

1. **Create Subdirectories**: One directory per content category
2. **Add Images**: Place representative images in their corresponding category folder
3. **Naming**: Image filenames don't matter (use any naming convention)
4. **Formats**: Supported formats: .jpg, .jpeg, .png, .bmp, .gif
5. **Size**: Minimum 224×224 pixels recommended
6. **Quality**: Use clear, representative images

## Minimum Requirements

- At least 50-100 images per category
- 500+ images total for good results
- Balanced distribution across categories (similar number per category)
- Clear, representative images for each category

## Example Structure

```
TrainingData/
├── Safe/
│   ├── landscape_01.jpg
│   ├── portrait_02.jpg
│   ├── object_03.png
│   └── ... (50-100 images)
├── Weapons/
│   ├── gun_01.jpg
│   ├── knife_02.jpg
│   ├── explosive_03.jpg
│   └── ... (50-100 images)
├── Violence_gore/
│   ├── fight_01.jpg
│   ├── injury_02.jpg
│   └── ... (50-100 images)
└── ... (other categories)
```

## Adding Training Data

1. Create the subdirectories matching the categories
2. Add images to each subdirectory
3. Ensure filenames have correct extensions (.jpg, .png, etc.)
4. Run the application to train the model
5. Monitor the console for training progress

## Best Practices

- Use diverse, representative images
- Avoid duplicate images
- Ensure images are properly classified in their directories
- Keep a separate test set for validation
- Retrain periodically with new data

## Notes

- The model will automatically discover images in these directories
- Training time depends on number of images and hardware
- Start with a small dataset and expand for better accuracy
- Monitor prediction accuracy and adjust as needed

For detailed training instructions, see [ML_TRAINING_GUIDE.md](../docs/ML_TRAINING_GUIDE.md)
