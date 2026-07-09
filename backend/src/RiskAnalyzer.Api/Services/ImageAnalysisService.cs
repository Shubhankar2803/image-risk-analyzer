using System.Text.RegularExpressions;

namespace RiskAnalyzer.Api.Services;

using RiskAnalyzer.Api.Data;
using RiskAnalyzer.Api.DTOs;
using RiskAnalyzer.Api.Models;
using RiskAnalyzer.Api.Repositories;
using RiskAnalyzer.Api.Config;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Net.Http.Headers;

/// <summary>
/// Service for handling image analysis operations
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
    private readonly IImageAnalysisRepository _repo;
    private readonly IUserRepository _userRepo;
    private readonly ILogger<ImageAnalysisService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly GeminiSettings _geminiSettings;

    public ImageAnalysisService(
        IImageAnalysisRepository repo,
        IUserRepository userRepo,
        ILogger<ImageAnalysisService> logger,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        GeminiSettings geminiSettings)
    {
        _repo = repo;
        _userRepo = userRepo;
        _logger = logger;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _geminiSettings = geminiSettings;
    }

    public async Task<ImageAnalysisResponseDto?> AnalyzeImageAsync(Guid userId, IFormFile file)
    {
        try
        {
            // Validate user exists
            var user = await _userRepo.GetUserByIdAsync(userId);
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

            // Add tags if provided
            if (geminiAnalysis.ContainsKey("tags"))
            {
                var tagsList = (List<Dictionary<string, dynamic>>)geminiAnalysis["tags"];
                foreach (var tagData in tagsList)
                {
                    var tag = new AnalysisTag
                    {
                        Id = Guid.NewGuid(),
                        ImageAnalysisId = analysis.Id,
                        Name = tagData.ContainsKey("name") ? tagData["name"] : "",
                        Category = tagData.ContainsKey("category") ? tagData["category"] : "",
                        Confidence = (int)(((decimal)tagData.GetValueOrDefault("confidence", 0m)) * 100),
                        Severity = tagData.ContainsKey("severity") ? tagData["severity"] : "Low"
                    };
                    analysis.Tags.Add(tag);
                }
            }

            await _repo.AddAsync(analysis);

            _logger.LogInformation("Image analysis created: {AnalysisId} with {TagCount} tags", analysis.Id, analysis.Tags.Count);

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
                                "text": "Analyze this image for potential risks including Privacy & PII (financial info, ID documents, faces, addresses), Safety & Violence (violence, weapons), and Adult Content. Respond ONLY with this exact JSON structure: {\"overallRiskScore\": 0-100, \"confidenceScore\": 0-100, \"primaryClassification\": \"Safe|Medium Risk|High Risk\", \"tags\": [{\"category\": \"Privacy & PII|Safety & Violence|Adult Content\", \"name\": \"specific tag name\", \"confidence\": 0-100, \"severity\": \"High|Medium|Low\"}], \"summary\": \"Brief finding\"}"
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

                    // Check if Gemini blocked the content due to safety filters
                    if (root.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() == 0)
                    {
                        _logger.LogWarning("Gemini API blocked content (empty candidates array)");
                        throw new InvalidOperationException("CONTENT_BLOCKED: Image contains content that violates safety policies");
                    }

                    // Check for finish reason indicating content was blocked
                    if (root.TryGetProperty("candidates", out candidates) && candidates.GetArrayLength() > 0)
                    {
                        var candidate = candidates[0];
                        if (candidate.TryGetProperty("finishReason", out var finishReason))
                        {
                            string reason = finishReason.GetString() ?? "";
                            if (reason.Equals("SAFETY", StringComparison.OrdinalIgnoreCase))
                            {
                                _logger.LogWarning("Gemini API blocked content due to safety filter");
                                throw new InvalidOperationException("CONTENT_BLOCKED: Image contains content that violates safety policies");
                            }
                        }
                    }

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

                        // Parse tags array from Gemini response
                        var tags = new List<Dictionary<string, dynamic>>();
                        if (analysis.TryGetProperty("tags", out var tagsElement) && tagsElement.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var tag in tagsElement.EnumerateArray())
                            {
                                var tagDict = new Dictionary<string, dynamic>();
                                if (tag.TryGetProperty("category", out var category))
                                    tagDict["category"] = category.GetString() ?? "";
                                if (tag.TryGetProperty("name", out var name))
                                    tagDict["name"] = name.GetString() ?? "";
                                if (tag.TryGetProperty("confidence", out var confidence))
                                    tagDict["confidence"] = decimal.Parse(confidence.GetRawText()) / 100m;
                                if (tag.TryGetProperty("severity", out var severity))
                                    tagDict["severity"] = severity.GetString() ?? "Low";
                                tags.Add(tagDict);
                            }
                        }

                        return new Dictionary<string, dynamic>
                        {
                            ["riskScore"] = decimal.Parse(analysis.GetProperty("overallRiskScore").GetRawText()) / 100m,
                            ["classification"] = analysis.GetProperty("primaryClassification").GetString() ?? "Safe",
                            ["analysisDetails"] = analysis.GetProperty("summary").GetString() ?? "Image analyzed",
                            ["confidenceScore"] = decimal.Parse(analysis.GetProperty("confidenceScore").GetRawText()) / 100m,
                            ["tags"] = tags
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
            
            if (ex.Message.Contains("CONTENT_BLOCKED"))
            {
                userMessage = "The image contains adult or explicit content that cannot be analyzed. Please upload a different image.";
            }
            else if (ex.Message.Contains("503") || ex.Message.Contains("unavailable") || ex.Message.Contains("high demand"))
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
                ["classification"] = ex.Message.Contains("CONTENT_BLOCKED") ? "Blocked - Explicit Content" : "Pending Analysis",
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
            var analyses = await _repo.GetByUserIdAsync(userId);
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
            var analysis = await _repo.GetByIdAsync(analysisId);
            if (analysis == null || analysis.UserId != userId) return null;
            return MapToDto(analysis);
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
            var analysis = await _repo.GetByIdAsync(analysisId);
            if (analysis == null || analysis.UserId != userId) return false;

            if (File.Exists(analysis.FilePath)) File.Delete(analysis.FilePath);

            await _repo.DeleteAsync(analysis);

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
        var tags = analysis.Tags.Select(t => new AnalysisTagDto
        {
            Category = t.Category,
            Name = t.Name,
            Confidence = t.Confidence / 100m,
            Severity = t.Severity
        }).ToList();

        var overallRiskScore = analysis.RiskScore;

        return new ImageAnalysisResponseDto
        {
            ImageId = analysis.Id,
            Id = analysis.Id, // Legacy field
            FileName = analysis.FileName,
            Status = "Completed",
            AnalysisResult = new ImageAnalysisResultDto
            {
                OverallRiskScore = overallRiskScore,
                Tags = tags
            },
            // Legacy fields
            FileSizeBytes = analysis.FileSizeBytes,
            RiskScore = analysis.RiskScore,
            Classification = analysis.Classification,
            AnalysisDetails = analysis.AnalysisDetails,
            ConfidenceScore = analysis.ConfidenceScore,
            AnalyzedAt = analysis.AnalyzedAt,
            CreatedAt = analysis.CreatedAt,
            Categories = analysis.Categories
        };
    }
}
