namespace RiskAnalyzer.Api.Services;

using RiskAnalyzer.Api.Config;
using RiskAnalyzer.Api.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Service for generating and validating JWT tokens
/// </summary>
public interface ITokenService
{
    string GenerateToken(User user);
    ClaimsPrincipal? ValidateToken(string token);

    /// <summary>
    /// Create and persist a refresh token for the provided user
    /// </summary>
    Task<string> CreateRefreshTokenAsync(User user);

    /// <summary>
    /// Validate a refresh token string and return the associated user if valid
    /// </summary>
    Task<User?> ValidateRefreshTokenAsync(string refreshToken);

    /// <summary>
    /// Rotate (revoke + replace) an existing refresh token and return a new token string
    /// </summary>
    Task<string> RotateRefreshTokenAsync(RiskAnalyzer.Api.Models.RefreshToken existingToken);
    
    /// <summary>
    /// Retrieve a refresh token entity by its token string
    /// </summary>
    Task<RiskAnalyzer.Api.Models.RefreshToken?> GetRefreshTokenAsync(string refreshToken);
}

/// <summary>
/// Implementation of Token Service using JWT
/// </summary>
public class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<TokenService> _logger;
    private readonly RiskAnalyzer.Api.Data.ApplicationDbContext _db;
    
    public TokenService(IOptions<JwtSettings> jwtSettings, ILogger<TokenService> logger, RiskAnalyzer.Api.Data.ApplicationDbContext db)
    {
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
        _db = db;
    }
    
    /// <summary>
    /// Generate JWT token for authenticated user
    /// Token includes user claims and expires after configured time
    /// </summary>
    public string GenerateToken(User user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));
        
        // Create claims for JWT
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim("FullName", user.FullName),
            new Claim("IsActive", user.IsActive.ToString())
        };
        
        // Create security key from secret
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        // Create token descriptor
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = credentials
        };
        
        // Generate token
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        
        _logger.LogInformation($"JWT token generated for user: {user.Username}");
        
        return tokenHandler.WriteToken(token);
    }
    
    /// <summary>
    /// Validate JWT token and return claims if valid
    /// Returns null if token is invalid or expired
    /// </summary>
    public ClaimsPrincipal? ValidateToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;
        
        try
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);
            
            return principal;
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Token validation failed: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Create and persist a cryptographically-strong refresh token associated with a user
    /// </summary>
    public async Task<string> CreateRefreshTokenAsync(User user)
    {
        // generate random 64-byte token and base64
        var bytes = new byte[64];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            rng.GetBytes(bytes);

        var token = Convert.ToBase64String(bytes);
        var refresh = new RiskAnalyzer.Api.Models.RefreshToken
        {
            Token = token,
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays)
        };

        _db.RefreshTokens.Add(refresh);
        await _db.SaveChangesAsync();

        _logger.LogInformation($"Refresh token created for user {user.Username}");
        return token;
    }

    public async Task<User?> ValidateRefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return null;

        var existing = await _db.RefreshTokens.FirstOrDefaultAsync(r => r.Token == refreshToken);
        if (existing == null || existing.IsRevoked || existing.ExpiresAt <= DateTime.UtcNow)
            return null;

        var user = await _db.Users.FindAsync(existing.UserId);
        return user;
    }

    public async Task<RiskAnalyzer.Api.Models.RefreshToken?> GetRefreshTokenAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return null;

        return await _db.RefreshTokens.FirstOrDefaultAsync(r => r.Token == refreshToken);
    }

    public async Task<string> RotateRefreshTokenAsync(RiskAnalyzer.Api.Models.RefreshToken existingToken)
    {
        // mark existing as revoked and generate a replacement
        existingToken.IsRevoked = true;
        existingToken.RevokedAt = DateTime.UtcNow;

        var bytes = new byte[64];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            rng.GetBytes(bytes);

        var newToken = Convert.ToBase64String(bytes);

        var replacement = new RiskAnalyzer.Api.Models.RefreshToken
        {
            Token = newToken,
            UserId = existingToken.UserId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays)
        };

        existingToken.ReplacedByToken = newToken;
        _db.RefreshTokens.Add(replacement);
        await _db.SaveChangesAsync();

        _logger.LogInformation($"Rotated refresh token for userId {existingToken.UserId}");
        return newToken;
    }
}
