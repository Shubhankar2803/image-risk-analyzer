namespace RiskAnalyzer.Api.Controllers;

using RiskAnalyzer.Api.DTOs;
using RiskAnalyzer.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

/// <summary>
/// Image analysis endpoints for uploading and retrieving image analyses
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ImageAnalysisController : ControllerBase
{
    private readonly IImageAnalysisService _imageAnalysisService;
    private readonly ILogger<ImageAnalysisController> _logger;

    public ImageAnalysisController(IImageAnalysisService imageAnalysisService, ILogger<ImageAnalysisController> logger)
    {
        _imageAnalysisService = imageAnalysisService;
        _logger = logger;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }
        throw new UnauthorizedAccessException("User ID not found in token");
    }

    /// <summary>
    /// Upload and analyze an image
    /// </summary>
    /// <param name="file">Image file to analyze</param>
    /// <returns>Image analysis results</returns>
    /// <response code="200">Image analyzed successfully</response>
    /// <response code="400">Invalid file</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="500">Server error</response>
    [HttpPost("upload")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<ImageAnalysisResponseDto>>> UploadImage([FromForm] IFormFile file)
    {
        try
        {
            var userId = GetUserId();

            if (file == null || file.Length == 0)
                return BadRequest(new ApiResponse<ImageAnalysisResponseDto>
                {
                    Success = false,
                    Error = new ApiError { Message = "No file provided", Code = "NO_FILE" }
                });

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            if (!allowedExtensions.Contains(fileExtension))
                return BadRequest(new ApiResponse<ImageAnalysisResponseDto>
                {
                    Success = false,
                    Error = new ApiError { Message = "Invalid file type", Code = "INVALID_FILE_TYPE" }
                });

            const long maxFileSize = 10 * 1024 * 1024; // 10 MB
            if (file.Length > maxFileSize)
                return BadRequest(new ApiResponse<ImageAnalysisResponseDto>
                {
                    Success = false,
                    Error = new ApiError { Message = "File too large (max 10 MB)", Code = "FILE_TOO_LARGE" }
                });

            var result = await _imageAnalysisService.AnalyzeImageAsync(userId, file);

            if (result == null)
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new ApiResponse<ImageAnalysisResponseDto>
                    {
                        Success = false,
                        Error = new ApiError { Message = "Failed to analyze image", Code = "ANALYSIS_FAILED" }
                    });

            return Ok(new ApiResponse<ImageAnalysisResponseDto>
            {
                Success = true,
                Data = result,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new ApiResponse<ImageAnalysisResponseDto>
            {
                Success = false,
                Error = new ApiError { Message = "Unauthorized", Code = "UNAUTHORIZED" }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ApiResponse<ImageAnalysisResponseDto>
                {
                    Success = false,
                    Error = new ApiError { Message = "Server error", Code = "SERVER_ERROR" }
                });
        }
    }

    /// <summary>
    /// Get all image analyses for current user
    /// </summary>
    /// <returns>List of image analyses</returns>
    /// <response code="200">Analyses retrieved successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="500">Server error</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<List<ImageAnalysisResponseDto>>>> GetMyAnalyses()
    {
        try
        {
            var userId = GetUserId();
            var analyses = await _imageAnalysisService.GetUserAnalysesAsync(userId);

            return Ok(new ApiResponse<List<ImageAnalysisResponseDto>>
            {
                Success = true,
                Data = analyses,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new ApiResponse<List<ImageAnalysisResponseDto>>
            {
                Success = false,
                Error = new ApiError { Message = "Unauthorized", Code = "UNAUTHORIZED" }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving analyses");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ApiResponse<List<ImageAnalysisResponseDto>>
                {
                    Success = false,
                    Error = new ApiError { Message = "Server error", Code = "SERVER_ERROR" }
                });
        }
    }

    /// <summary>
    /// Get a specific image analysis
    /// </summary>
    /// <param name="id">Analysis ID</param>
    /// <returns>Image analysis details</returns>
    /// <response code="200">Analysis retrieved successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Analysis not found</response>
    /// <response code="500">Server error</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<ImageAnalysisResponseDto>>> GetAnalysis(Guid id)
    {
        try
        {
            var userId = GetUserId();
            var analysis = await _imageAnalysisService.GetAnalysisByIdAsync(id, userId);

            if (analysis == null)
                return NotFound(new ApiResponse<ImageAnalysisResponseDto>
                {
                    Success = false,
                    Error = new ApiError { Message = "Analysis not found", Code = "NOT_FOUND" }
                });

            return Ok(new ApiResponse<ImageAnalysisResponseDto>
            {
                Success = true,
                Data = analysis,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new ApiResponse<ImageAnalysisResponseDto>
            {
                Success = false,
                Error = new ApiError { Message = "Unauthorized", Code = "UNAUTHORIZED" }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving analysis {AnalysisId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ApiResponse<ImageAnalysisResponseDto>
                {
                    Success = false,
                    Error = new ApiError { Message = "Server error", Code = "SERVER_ERROR" }
                });
        }
    }

    /// <summary>
    /// Delete an image analysis
    /// </summary>
    /// <param name="id">Analysis ID</param>
    /// <returns>Success message</returns>
    /// <response code="200">Analysis deleted successfully</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="404">Analysis not found</response>
    /// <response code="500">Server error</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> DeleteAnalysis(Guid id)
    {
        try
        {
            var userId = GetUserId();
            var deleted = await _imageAnalysisService.DeleteAnalysisAsync(id, userId);

            if (!deleted)
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Error = new ApiError { Message = "Analysis not found", Code = "NOT_FOUND" }
                });

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = new { Message = "Analysis deleted successfully" },
                Timestamp = DateTime.UtcNow
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new ApiResponse<object>
            {
                Success = false,
                Error = new ApiError { Message = "Unauthorized", Code = "UNAUTHORIZED" }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting analysis {AnalysisId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ApiResponse<object>
                {
                    Success = false,
                    Error = new ApiError { Message = "Server error", Code = "SERVER_ERROR" }
                });
        }
    }
}
