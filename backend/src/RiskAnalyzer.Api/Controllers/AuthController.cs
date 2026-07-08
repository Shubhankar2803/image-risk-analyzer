namespace RiskAnalyzer.Api.Controllers;

using RiskAnalyzer.Api.DTOs;
using RiskAnalyzer.Api.Services;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Authentication endpoints for user login and registration
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthController> _logger;
    private readonly RiskAnalyzer.Api.Config.JwtSettings _jwtSettings;

    public AuthController(IUserService userService, ITokenService tokenService, ILogger<AuthController> logger, IOptions<RiskAnalyzer.Api.Config.JwtSettings> jwtOptions)
    {
        _userService = userService;
        _tokenService = tokenService;
        _logger = logger;
        _jwtSettings = jwtOptions.Value;
    }
    
    /// <summary>
    /// Register a new user
    /// </summary>
    /// <param name="request">Registration details (email, username, password, fullName)</param>
    /// <returns>Success message with new user info</returns>
    /// <response code="200">User registered successfully</response>
    /// <response code="400">Invalid input or user already exists</response>
    /// <response code="500">Server error</response>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            // Validation
            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<AuthResponse>
                {
                    Success = false,
                    Error = new ApiError { Message = "Invalid input", Code = "VALIDATION_ERROR" }
                });
            
            // Register user
            var user = await _userService.RegisterUserAsync(
                request.Email,
                request.Username,
                request.Password,
                request.FullName
            );
            
            // Generate JWT token
                var token = _tokenService.GenerateToken(user);
                var refresh = await _tokenService.CreateRefreshTokenAsync(user);
            
            // Convert to DTO (never expose internal fields)
            var userDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                FullName = user.FullName,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                IsActive = user.IsActive
            };
            
            // Build response with token
                // Build response with token and refresh token
                var response = new AuthResponse
                {
                    Token = token,
                    User = userDto,
                    ExpiresIn = _jwtSettings.ExpirationMinutes,
                    TokenType = "Bearer",
                    RefreshToken = refresh,
                    RefreshExpiresIn = _jwtSettings.RefreshTokenExpirationDays * 24 * 60
                };
            
            _logger.LogInformation($"User registered successfully: {user.Username}");
            
            return Ok(new ApiResponse<AuthResponse>
            {
                Success = true,
                Data = response,
                Error = null
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning($"Registration failed: {ex.Message}");
            return BadRequest(new ApiResponse<AuthResponse>
            {
                Success = false,
                Error = new ApiError
                {
                    Message = ex.Message,
                    Code = "USER_EXISTS"
                }
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning($"Registration validation failed: {ex.Message}");
            return BadRequest(new ApiResponse<AuthResponse>
            {
                Success = false,
                Error = new ApiError
                {
                    Message = ex.Message,
                    Code = "INVALID_INPUT"
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Registration error: {ex.Message}");
            return StatusCode(500, new ApiResponse<AuthResponse>
            {
                Success = false,
                Error = new ApiError
                {
                    Message = "An error occurred during registration",
                    Details = ex.Message,
                    Code = "REGISTRATION_ERROR"
                }
            });
        }
    }
    
    /// <summary>
    /// Login user and return JWT token
    /// </summary>
    /// <param name="request">Login credentials (email and password)</param>
    /// <returns>JWT token, user info, and expiration time</returns>
    /// <response code="200">Login successful, token returned</response>
    /// <response code="401">Invalid credentials</response>
    /// <response code="400">Invalid input</response>
    /// <response code="500">Server error</response>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
    {
        try
        {
            // Validation
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new ApiResponse<AuthResponse>
                {
                    Success = false,
                    Error = new ApiError
                    {
                        Message = "Email and password are required",
                        Code = "VALIDATION_ERROR"
                    }
                });
            
            // Authenticate user
            var user = await _userService.AuthenticateAsync(request.Email, request.Password);
            
            if (user == null)
            {
                _logger.LogWarning($"Login failed for email: {request.Email} - Invalid credentials");
                return Unauthorized(new ApiResponse<AuthResponse>
                {
                    Success = false,
                    Error = new ApiError
                    {
                        Message = "Invalid email or password",
                        Code = "INVALID_CREDENTIALS"
                    }
                });
            }
            
            // Generate JWT token
            var token = _tokenService.GenerateToken(user);
            var refresh = await _tokenService.CreateRefreshTokenAsync(user);
            
            // Prepare user DTO (exclude sensitive data)
            var userDto = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Username,
                FullName = user.FullName,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                IsActive = user.IsActive
            };
            
            // Build response
            var response = new AuthResponse
            {
                Token = token,
                User = userDto,
                ExpiresIn = _jwtSettings.ExpirationMinutes,
                TokenType = "Bearer",
                RefreshToken = refresh,
                RefreshExpiresIn = _jwtSettings.RefreshTokenExpirationDays * 24 * 60
            };
            
            _logger.LogInformation($"User logged in successfully: {user.Username}");
            
            return Ok(new ApiResponse<AuthResponse>
            {
                Success = true,
                Data = response,
                Error = null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Login error: {ex.Message}");
            return StatusCode(500, new ApiResponse<AuthResponse>
            {
                Success = false,
                Error = new ApiError
                {
                    Message = "An error occurred during login",
                    Details = ex.Message,
                    Code = "LOGIN_ERROR"
                }
            });
        }
    }
    
    /// <summary>
    /// Health check endpoint to verify auth service is running
    /// </summary>
    /// <summary>
    /// Exchange a refresh token for a new access token (and rotate refresh token)
    /// </summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Refresh([FromBody] RefreshRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.RefreshToken))
            return BadRequest(new ApiResponse<AuthResponse> { Success = false, Error = new ApiError { Message = "Refresh token required", Code = "VALIDATION_ERROR" } });

        var existing = await _tokenService.GetRefreshTokenAsync(request.RefreshToken);
        if (existing == null || existing.IsRevoked || existing.ExpiresAt <= DateTime.UtcNow)
            return Unauthorized(new ApiResponse<AuthResponse> { Success = false, Error = new ApiError { Message = "Invalid or expired refresh token", Code = "INVALID_REFRESH" } });

        var user = await _userService.GetUserByIdAsync(existing.UserId);
        if (user == null)
            return Unauthorized(new ApiResponse<AuthResponse> { Success = false, Error = new ApiError { Message = "User not found", Code = "INVALID_REFRESH" } });

        // Create new JWT and rotate refresh token
        var newJwt = _tokenService.GenerateToken(user);
        var newRefresh = await _tokenService.RotateRefreshTokenAsync(existing);

        var userDto = new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Username = user.Username,
            FullName = user.FullName,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            IsActive = user.IsActive
        };

        var response = new AuthResponse
        {
            Token = newJwt,
            User = userDto,
            ExpiresIn = _jwtSettings.ExpirationMinutes,
            TokenType = "Bearer",
            RefreshToken = newRefresh,
            RefreshExpiresIn = _jwtSettings.RefreshTokenExpirationDays * 24 * 60
        };

        return Ok(new ApiResponse<AuthResponse> { Success = true, Data = response });
    }

    /// <summary>
    /// Revoke a refresh token (logout)
    /// </summary>
    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse<string>>> Logout([FromBody] RefreshRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.RefreshToken))
            return BadRequest(new ApiResponse<string> { Success = false, Error = new ApiError { Message = "Refresh token required", Code = "VALIDATION_ERROR" } });

        var existing = await _tokenService.GetRefreshTokenAsync(request.RefreshToken);
        if (existing == null)
            return Ok(new ApiResponse<string> { Success = true, Data = "No-op" });

        existing.IsRevoked = true;
        existing.RevokedAt = DateTime.UtcNow;

        var db = (RiskAnalyzer.Api.Data.ApplicationDbContext)HttpContext.RequestServices.GetService(typeof(RiskAnalyzer.Api.Data.ApplicationDbContext));
        if (db != null)
        {
            db.RefreshTokens.Update(existing);
            await db.SaveChangesAsync();
        }

        return Ok(new ApiResponse<string> { Success = true, Data = "Logged out" });
    }
    [HttpGet("ping")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<string>> Ping()
    {
        return Ok(new ApiResponse<string>
        {
            Success = true,
            Data = "Auth service is running"
        });
    }
}
