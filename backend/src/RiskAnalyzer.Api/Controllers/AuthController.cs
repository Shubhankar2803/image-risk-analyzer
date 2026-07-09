namespace RiskAnalyzer.Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using RiskAnalyzer.Api.Config;
    using RiskAnalyzer.Api.DTOs;
    using RiskAnalyzer.Api.Models;
    using RiskAnalyzer.Api.Services;

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITokenService _tokenService;
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IUserService userService, ITokenService tokenService, IOptions<JwtSettings> jwtOptions, ILogger<AuthController> logger)
        {
            _userService = userService;
            _tokenService = tokenService;
            _jwtSettings = jwtOptions.Value;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var user = await _userService.RegisterUserAsync(request.Email, request.Username, request.Password, request.FullName);
                var token = _tokenService.GenerateToken(user);

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
                    Token = token,
                    User = userDto,
                    ExpiresIn = _jwtSettings.ExpirationMinutes,
                    TokenType = "Bearer"
                };

                _logger.LogInformation($"User registered successfully: {user.Username}");
                return Ok(new ApiResponse<AuthResponse> { Success = true, Data = response });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning($"Registration failed: {ex.Message}");
                return BadRequest(new ApiResponse<AuthResponse> { Success = false, Error = new ApiError { Message = ex.Message, Code = "USER_EXISTS" } });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning($"Registration validation failed: {ex.Message}");
                return BadRequest(new ApiResponse<AuthResponse> { Success = false, Error = new ApiError { Message = ex.Message, Code = "INVALID_INPUT" } });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Registration error: {ex.Message}");
                return StatusCode(500, new ApiResponse<AuthResponse> { Success = false, Error = new ApiError { Message = "An error occurred during registration", Details = ex.Message, Code = "REGISTRATION_ERROR" } });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                    return BadRequest(new ApiResponse<AuthResponse> { Success = false, Error = new ApiError { Message = "Email and password are required", Code = "VALIDATION_ERROR" } });

                var user = await _userService.AuthenticateAsync(request.Email, request.Password);
                if (user == null)
                    return Unauthorized(new ApiResponse<AuthResponse> { Success = false, Error = new ApiError { Message = "Invalid email or password", Code = "INVALID_CREDENTIALS" } });

                var token = _tokenService.GenerateToken(user);

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
                    Token = token,
                    User = userDto,
                    ExpiresIn = _jwtSettings.ExpirationMinutes,
                    TokenType = "Bearer"
                };

                _logger.LogInformation($"User logged in successfully: {user.Username}");
                return Ok(new ApiResponse<AuthResponse> { Success = true, Data = response });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Login error: {ex.Message}");
                return StatusCode(500, new ApiResponse<AuthResponse> { Success = false, Error = new ApiError { Message = "An error occurred during login", Details = ex.Message, Code = "LOGIN_ERROR" } });
            }
        }

        [HttpGet("ping")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<ApiResponse<string>> Ping()
        {
            return Ok(new ApiResponse<string> { Success = true, Data = "Auth service is running" });
        }
    }
}
