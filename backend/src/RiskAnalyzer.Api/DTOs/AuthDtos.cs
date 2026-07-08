namespace RiskAnalyzer.Api.DTOs;

public class LoginRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}

public class RegisterRequest
{
    public string Email { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FullName { get; set; } = null!;
}

public class AuthResponse
{
    public string Token { get; set; } = null!;
    public UserDto User { get; set; } = null!;
    public int ExpiresIn { get; set; }
    public string TokenType { get; set; } = "Bearer";
    public string? RefreshToken { get; set; }
    public int? RefreshExpiresIn { get; set; }
}

public class RefreshRequest
{
    public string RefreshToken { get; set; } = null!;
}

public class ApiError
{
    public string Message { get; set; } = null!;
    public string? Details { get; set; }
    public string? Code { get; set; }
}

public class ApiResponse<T>
{
    public T? Data { get; set; }
    public bool Success { get; set; }
    public ApiError? Error { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
