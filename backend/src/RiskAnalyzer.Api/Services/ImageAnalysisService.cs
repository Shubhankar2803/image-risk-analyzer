namespace RiskAnalyzer.Api.Services;

using RiskAnalyzer.Api.Data;
using RiskAnalyzer.Api.DTOs;
using RiskAnalyzer.Api.Models;
using RiskAnalyzer.Api.Config;
using RiskAnalyzer.Api.ML;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Service for handling image analysis operations using ML.NET fine-tuned model
/// with database-driven risk scoring
/// </summary>
public interface IImageAnalysisService
{
    Task<ImageAnalysisResponseDto?> AnalyzeImageAsync(Guid userId, IFormFile file);
    Task<List<ImageAnalysisResponseDto>> GetUserAnalysesAsync(Guid userId);
    Task<ImageAnalysisResponseDto?> GetAnalysisByIdAsync(Guid analysisId, Guid userId);
    Task<bool> DeleteAnalysisAsync(Guid analysisId, Guid userId);
}

public class ImageAnalysisService : IImageAnalysisService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ImageAnalysisService> _logger;
    private readonly IMLModelService _mlModelService;
    private readonly IRiskScoringService _riskScoringService;

    public ImageAnalysisService(
        ApplicationDbContext context, 
        ILogger<ImageAnalysisService> logger, 
        IMLModelService mlModelService,
        IRiskScoringService riskScoringService)
    {
        _context = context;
        _logger = logger;
        _mlModelService = mlModelService;
        _riskScoringService = riskScoringService;
    }

    public async Task<ImageAnalysisResponseDto?> AnalyzeImageAsync(Guid userId, IFormFile file)
    {
        try
        {
            // Validate user exists
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found", userId);
                return null;
            }

            // Validate file
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("Invalid file provided");
                return null;
            }

            // Validate model is loaded
            if (!_mlModelService.IsModelLoaded)
            {
                _logger.LogError("ML model not loaded");
                return null;
            }

            // Create upload directory if doesn't exist
            var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            Directory.CreateDirectory(uploadDir);

            // Generate unique filename
            var filename = $"{Guid.NewGuid()}_{file.FileName}";
            var filepath = Path.Combine(uploadDir, filename);

            // Save file
            using (var stream = new FileStream(filepath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            _logger.LogInformation("Image saved: {FilePath}", filepath);

            // Perform ML model inference (gets category and confidence)
            var mlPrediction = await _mlModelService.PredictImageAsync(filepath);

            // Calculate risk score using database-driven severity weights
            var riskResult = await _riskScoringService.CalculateRiskAsync(mlPrediction.Category, mlPrediction.Confidence);

            // Create analysis record with complete risk information
            var analysis = new ImageAnalysis
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FileName = file.FileName,
                FilePath = filepath,
                FileSizeBytes = file.Length,
                ContentType = file.ContentType ?? "application/octet-stream",
                RiskScore = riskResult.FinalRiskScore,
                Classification = riskResult.Category,
                AnalysisDetails = $"ML.NET Analysis: Category={riskResult.Category}, Confidence={riskResult.Confidence:P0}, Action={riskResult.Action}",
                ConfidenceScore = riskResult.Confidence,
                SeverityWeight = riskResult.SeverityWeight,
                RiskAction = riskResult.Action,
                RiskColor = riskResult.RiskColor,
                AnalyzedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _context.ImageAnalyses.Add(analysis);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Image analysis created: {AnalysisId} | Category={Category} | Confidence={Confidence:P0} | SeverityWeight={Weight} | FinalRiskScore={RiskScore:F2} | Action={Action}",
                analysis.Id, riskResult.Category, riskResult.Confidence, riskResult.SeverityWeight, riskResult.FinalRiskScore, riskResult.Action);

            return MapToDto(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing image");
            return null;
        }
    }

    public async Task<List<ImageAnalysisResponseDto>> GetUserAnalysesAsync(Guid userId)
    {
        try
        {
            var analyses = await _context.ImageAnalyses
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return analyses.Select(MapToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user analyses");
            return new List<ImageAnalysisResponseDto>();
        }
    }

    public async Task<ImageAnalysisResponseDto?> GetAnalysisByIdAsync(Guid analysisId, Guid userId)
    {
        try
        {
            var analysis = await _context.ImageAnalyses
                .FirstOrDefaultAsync(a => a.Id == analysisId && a.UserId == userId);

            return analysis != null ? MapToDto(analysis) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving analysis");
            return null;
        }
    }

    public async Task<bool> DeleteAnalysisAsync(Guid analysisId, Guid userId)
    {
        try
        {
            var analysis = await _context.ImageAnalyses
                .FirstOrDefaultAsync(a => a.Id == analysisId && a.UserId == userId);

            if (analysis == null)
                return false;

            // Delete physical file if exists
            if (File.Exists(analysis.FilePath))
            {
                File.Delete(analysis.FilePath);
            }

            _context.ImageAnalyses.Remove(analysis);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Analysis deleted: {AnalysisId}", analysisId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting analysis");
            return false;
        }
    }

    private ImageAnalysisResponseDto MapToDto(ImageAnalysis analysis)
    {
        return new ImageAnalysisResponseDto
        {
            Id = analysis.Id,
            FileName = analysis.FileName,
            FileSizeBytes = analysis.FileSizeBytes,
            RiskScore = analysis.RiskScore,
            Classification = analysis.Classification,
            AnalysisDetails = analysis.AnalysisDetails,
            ConfidenceScore = analysis.ConfidenceScore,
            SeverityWeight = analysis.SeverityWeight,
            RiskAction = analysis.RiskAction,
            RiskColor = analysis.RiskColor,
            AnalyzedAt = analysis.AnalyzedAt,
            Categories = analysis.Categories
        };
    }
}

    public async Task<ImageAnalysisResponseDto?> AnalyzeImageAsync(Guid userId, IFormFile file)
    {
        try
        {
            // Validate user exists
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found", userId);
                return null;
            }

            // Validate file
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("Invalid file provided");
                return null;
            }

            // Create upload directory if doesn't exist
            var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            Directory.CreateDirectory(uploadDir);

            // Generate unique filename
            var filename = $"{Guid.NewGuid()}_{file.FileName}";
            var filepath = Path.Combine(uploadDir, filename);

            // Save file
            using (var stream = new FileStream(filepath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Convert file to base64
            byte[] fileBytes;
            using (var stream = file.OpenReadStream())
            {
                using (var memoryStream = new MemoryStream())
                {
                    await stream.CopyToAsync(memoryStream);
                    fileBytes = memoryStream.ToArray();
                }
            }
            var base64Image = Convert.ToBase64String(fileBytes);

            // Call Gemini API for analysis
            var geminiAnalysis = await AnalyzeWithGeminiAsync(base64Image, file.ContentType ?? "image/jpeg");

            // Create analysis record
            var analysis = new ImageAnalysis
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FileName = file.FileName,
                FilePath = filepath,
                FileSizeBytes = file.Length,
                ContentType = file.ContentType ?? "application/octet-stream",
                RiskScore = geminiAnalysis["riskScore"],
                Classification = geminiAnalysis["classification"],
                AnalysisDetails = geminiAnalysis["analysisDetails"],
                ConfidenceScore = geminiAnalysis["confidenceScore"],
                Categories = geminiAnalysis.ContainsKey("categories") ? geminiAnalysis["categories"] : null,
                AnalyzedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _context.ImageAnalyses.Add(analysis);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Image analysis created: {AnalysisId}", analysis.Id);

            return MapToDto(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing image");
            return null;
        }
    }

    private async Task<Dictionary<string, dynamic>> AnalyzeWithGeminiAsync(string base64Image, string mediaType)
    {
        try
        {
            // Compress image to reduce API usage and avoid rate limits
            string compressedBase64 = CompressBase64Image(base64Image);
            _logger.LogInformation("Image compressed. Original size: {OriginalSize}, Compressed size: {CompressedSize}",
                base64Image.Length, compressedBase64.Length);

            var client = _httpClientFactory.CreateClient();
            var apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_geminiSettings.ApiKey}";

            // Simplified prompt to reduce token usage
            var jsonRequest = $$"""
            {
                "contents": [
                    {
                        "parts": [
                            {
                                "text": "Analyze image for safety risks. Respond ONLY with JSON: {\"primaryClassification\": \"Safe|Violence & Physical Harm|Harassment & Hate Speech|Sexually Explicit Content|Dangerous Activities|Sensitive Information\", \"overallRiskScore\": 0-100, \"confidenceScore\": 0-100, \"summary\": \"Brief finding\"}"
                            },
                            {
                                "inlineData": {
                                    "mimeType": "{{mediaType}}",
                                    "data": "{{compressedBase64}}"
                                }
                            }
                        ]
                    }
                ]
            }
            """;

            // Retry logic with exponential backoff
            int maxRetries = 3;
            int retryDelayMs = 1000;

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    var content = new StringContent(jsonRequest, System.Text.Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(apiUrl, content);

                    var responseJson = await response.Content.ReadAsStringAsync();
                    
                    _logger.LogInformation("Gemini API Request attempt {Attempt}/{MaxRetries}", attempt, maxRetries);
                    _logger.LogInformation("Gemini API Response status: {StatusCode}", response.StatusCode);
                    _logger.LogInformation("Gemini API Response length: {Length} bytes", responseJson.Length);

                    int statusCode = (int)response.StatusCode;
                    bool isRetryableError = (statusCode == 429 || statusCode == 503) && attempt < maxRetries;

                    if (isRetryableError)
                    {
                        string reason = statusCode == 429 ? "Rate limited (429)" : "Service unavailable (503)";
                        _logger.LogWarning("{Reason}. Retry attempt {Attempt}/{MaxRetries} after {Delay}ms", 
                            reason, attempt, maxRetries, retryDelayMs);
                        await Task.Delay(retryDelayMs);
                        retryDelayMs *= 2; // Exponential backoff
                        continue;
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogError("Gemini API error response: {StatusCode} {ReasonPhrase}", 
                            statusCode, response.ReasonPhrase);
                        _logger.LogError("Response body: {ResponseBody}", responseJson.Substring(0, Math.Min(500, responseJson.Length)));
                        response.EnsureSuccessStatusCode();
                    }

                    _logger.LogInformation("Gemini API Response successful on attempt {Attempt}", attempt);

                    var jsonDoc = JsonDocument.Parse(responseJson);
                    var root = jsonDoc.RootElement;

                    // Extract text from Gemini response
                    var textContent = root
                        .GetProperty("candidates")[0]
                        .GetProperty("content")
                        .GetProperty("parts")[0]
                        .GetProperty("text")
                        .GetString();

                    _logger.LogInformation("Gemini Analysis Text: {Text}", textContent);

                    // Parse the JSON response from Gemini
                    if (!string.IsNullOrEmpty(textContent))
                    {
                        // Extract JSON from markdown code blocks if present
                        var jsonContent = ExtractJsonFromMarkdown(textContent);
                        var analysisJson = JsonDocument.Parse(jsonContent);
                        var analysis = analysisJson.RootElement;

                        return new Dictionary<string, dynamic>
                        {
                            ["riskScore"] = analysis.GetProperty("overallRiskScore").GetInt32(),
                            ["classification"] = analysis.GetProperty("primaryClassification").GetString() ?? "Safe",
                            ["analysisDetails"] = analysis.GetProperty("summary").GetString() ?? "Image analyzed",
                            ["confidenceScore"] = analysis.GetProperty("confidenceScore").GetInt32()
                        };
                    }

                    throw new Exception("No response from Gemini API");
                }
                catch (HttpRequestException ex) when (((int?)ex.StatusCode == 429 || (int?)ex.StatusCode == 503) && attempt < maxRetries)
                {
                    int? code = (int?)ex.StatusCode;
                    string reason = code == 429 ? "Rate limit error" : "Service unavailable error";
                    _logger.LogWarning("{Reason} on attempt {Attempt}. Retrying after {Delay}ms", 
                        reason, attempt, retryDelayMs);
                    await Task.Delay(retryDelayMs);
                    retryDelayMs *= 2;
                }
            }

            throw new Exception("Failed after maximum retry attempts");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Gemini API: {ErrorMessage}", ex.Message);
            _logger.LogError("Stack trace: {StackTrace}", ex.StackTrace);
            
            // Return friendly error message for user
            string userMessage = "Gemini API service is temporarily unavailable. Your image has been saved and will be analyzed automatically when the service recovers.";
            if (ex.Message.Contains("503") || ex.Message.Contains("unavailable") || ex.Message.Contains("high demand"))
            {
                userMessage = "Gemini API is experiencing high demand. Your image has been saved and will be analyzed automatically. Please try again in a few minutes.";
            }
            else if (ex.Message.Contains("401") || ex.Message.Contains("403"))
            {
                userMessage = "API authentication failed. Please check your Gemini API key in the server configuration.";
            }
            else if (ex.Message.Contains("429"))
            {
                userMessage = "Too many requests to Gemini API. Your image has been saved. Please retry in a few moments.";
            }

            return new Dictionary<string, dynamic>
            {
                ["riskScore"] = 0,
                ["classification"] = "Pending Analysis",
                ["analysisDetails"] = userMessage,
                ["confidenceScore"] = 0
            };
        }
    }

    private string ExtractJsonFromMarkdown(string content)
    {
        var match = Regex.Match(content, @"```(?:json)?\s*([\s\S]*?)\s*```");
        if (match.Success)
            return match.Groups[1].Value.Trim();
        throw new InvalidOperationException("No JSON code block found in response");
    }

#pragma warning disable CA1416
    private string CompressBase64Image(string base64Image)
    {
        try
        {
            byte[] imageBytes = Convert.FromBase64String(base64Image);
            
            // If already small, return as-is
            if (imageBytes.Length < 100000) // Less than 100KB
                return base64Image;

            _logger.LogInformation("Compressing image from {Size} bytes", imageBytes.Length);

            using (var ms = new MemoryStream(imageBytes))
            {
                using (var image = System.Drawing.Image.FromStream(ms))
                {
                    // Resize if too large
                    int maxWidth = 800;
                    int maxHeight = 800;
                    
                    if (image.Width > maxWidth || image.Height > maxHeight)
                    {
                        double scaleRatio = Math.Min((double)maxWidth / image.Width, (double)maxHeight / image.Height);
                        int newWidth = (int)(image.Width * scaleRatio);
                        int newHeight = (int)(image.Height * scaleRatio);
                        
                        using (var resized = new System.Drawing.Bitmap(newWidth, newHeight))
                        {
                            using (var graphics = System.Drawing.Graphics.FromImage(resized))
                            {
                                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                                graphics.DrawImage(image, 0, 0, newWidth, newHeight);
                            }

                            using (var outputMs = new MemoryStream())
                            {
                                var qualityEncoder = System.Drawing.Imaging.Encoder.Quality;
                                var quality = new System.Drawing.Imaging.EncoderParameter(qualityEncoder, 60L);
                                var codecParams = new System.Drawing.Imaging.EncoderParameters(1);
                                codecParams.Param[0] = quality;

                                var jpgCodec = System.Drawing.Imaging.ImageCodecInfo.GetImageEncoders()
                                    .FirstOrDefault(x => x.FormatID == System.Drawing.Imaging.ImageFormat.Jpeg.Guid);

                                resized.Save(outputMs, jpgCodec, codecParams);
                                string compressed = Convert.ToBase64String(outputMs.ToArray());
                                _logger.LogInformation("Image compressed to {Size} bytes", outputMs.ToArray().Length);
                                return compressed;
                            }
                        }
                    }
                }
            }

            return base64Image;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Image compression failed, using original");
            return base64Image;
        }
    }
#pragma warning restore CA1416

    private Dictionary<string, dynamic> GetMockAnalysis()
    {
        return new Dictionary<string, dynamic>
        {
            ["riskScore"] = new Random().Next(20, 80),
            ["classification"] = new[] { "Safe", "Moderate Risk", "High Risk" }[new Random().Next(3)],
            ["analysisDetails"] = "Automated analysis result",
            ["confidenceScore"] = new Random().Next(70, 95)
        };
    }

    public async Task<List<ImageAnalysisResponseDto>> GetUserAnalysesAsync(Guid userId)
    {
        try
        {
            var analyses = await _context.ImageAnalyses
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return analyses.Select(MapToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user analyses");
            return new List<ImageAnalysisResponseDto>();
        }
    }

    public async Task<ImageAnalysisResponseDto?> GetAnalysisByIdAsync(Guid analysisId, Guid userId)
    {
        try
        {
            var analysis = await _context.ImageAnalyses
                .FirstOrDefaultAsync(a => a.Id == analysisId && a.UserId == userId);

            return analysis != null ? MapToDto(analysis) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving analysis");
            return null;
        }
    }

    public async Task<bool> DeleteAnalysisAsync(Guid analysisId, Guid userId)
    {
        try
        {
            var analysis = await _context.ImageAnalyses
                .FirstOrDefaultAsync(a => a.Id == analysisId && a.UserId == userId);

            if (analysis == null)
                return false;

            // Delete physical file if exists
            if (File.Exists(analysis.FilePath))
            {
                File.Delete(analysis.FilePath);
            }

            _context.ImageAnalyses.Remove(analysis);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Analysis deleted: {AnalysisId}", analysisId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting analysis");
            return false;
        }
    }

    private ImageAnalysisResponseDto MapToDto(ImageAnalysis analysis)
    {
        return new ImageAnalysisResponseDto
        {
            Id = analysis.Id,
            FileName = analysis.FileName,
            FileSizeBytes = analysis.FileSizeBytes,
            RiskScore = analysis.RiskScore,
            Classification = analysis.Classification,
            AnalysisDetails = analysis.AnalysisDetails,
            ConfidenceScore = analysis.ConfidenceScore,
            AnalyzedAt = analysis.AnalyzedAt,
            Categories = analysis.Categories
        };
    }
}
