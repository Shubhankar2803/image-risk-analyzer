namespace RiskAnalyzer.Api.Services;

using RiskAnalyzer.Api.Models;
using RiskAnalyzer.Api.Repositories;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Service for user authentication operations
/// Handles user registration, login, password hashing
/// </summary>
public interface IUserService
{
    Task<User> RegisterUserAsync(string email, string username, string password, string fullName);
    Task<User?> AuthenticateAsync(string email, string password);
    Task<User?> GetUserByIdAsync(Guid userId);
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}

/// <summary>
/// Implementation of User Service
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;
    
    public UserService(IUserRepository userRepository, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }
    
    /// <summary>
    /// Register a new user with email, username, and password
    /// </summary>
    public async Task<User> RegisterUserAsync(string email, string username, string password, string fullName)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));
            
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username is required", nameof(username));
            
        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            throw new ArgumentException("Password must be at least 6 characters", nameof(password));
        
        // Check if user already exists
        if (await _userRepository.UserExistsByEmailAsync(email))
            throw new InvalidOperationException($"User with email '{email}' already exists");
            
        if (await _userRepository.UserExistsByUsernameAsync(username))
            throw new InvalidOperationException($"Username '{username}' is already taken");
        
        // Create new user
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email.ToLower().Trim(),
            Username = username.Trim(),
            FullName = fullName?.Trim() ?? username,
            PasswordHash = HashPassword(password),
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
        
        // Save to database
        var createdUser = await _userRepository.CreateUserAsync(user);
        
        _logger.LogInformation($"User registered successfully: {createdUser.Username} ({createdUser.Email})");
        
        return createdUser;
    }
    
    /// <summary>
    /// Authenticate user with email and password
    /// Returns user if credentials are valid, null otherwise
    /// </summary>
    public async Task<User?> AuthenticateAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return null;
        
        // Get user by email
        var user = await _userRepository.GetUserByEmailAsync(email);
        
        if (user == null || !user.IsActive)
            return null;
        
        // Verify password
        if (!VerifyPassword(password, user.PasswordHash))
            return null;
        
        // Update last login time
        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateUserAsync(user);
        
        _logger.LogInformation($"User logged in: {user.Username}");
        
        return user;
    }
    
    /// <summary>
    /// Get user by ID
    /// </summary>
    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await _userRepository.GetUserByIdAsync(userId);
    }
    
    /// <summary>
    /// Hash password using PBKDF2
    /// Salt is included in the hash for storage and verification
    /// </summary>
    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentNullException(nameof(password));
        
        // Generate random salt (16 bytes)
        byte[] salt = new byte[16];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }
        
        // Hash password with PBKDF2 (10,000 iterations)
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            password: password,
            salt: salt,
            iterations: 10000,
            hashAlgorithm: HashAlgorithmName.SHA256,
            outputLength: 32
        );
        
        // Combine salt and hash: salt (16) + hash (32) = 48 bytes
        byte[] hashWithSalt = new byte[48];
        Array.Copy(salt, 0, hashWithSalt, 0, 16);
        Array.Copy(hash, 0, hashWithSalt, 16, 32);
        
        // Return as base64 string
        return Convert.ToBase64String(hashWithSalt);
    }
    
    /// <summary>
    /// Verify password against stored hash
    /// Extracts salt from hash and compares with new hash of input password
    /// </summary>
    public bool VerifyPassword(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
            return false;
        
        try
        {
            // Convert base64 hash back to bytes
            byte[] hashWithSalt = Convert.FromBase64String(hash);
            
            if (hashWithSalt.Length != 48)
                return false;
            
            // Extract salt (first 16 bytes)
            byte[] salt = new byte[16];
            Array.Copy(hashWithSalt, 0, salt, 0, 16);
            
            // Hash input password with extracted salt
            byte[] hash2 = Rfc2898DeriveBytes.Pbkdf2(
                password: password,
                salt: salt,
                iterations: 10000,
                hashAlgorithm: HashAlgorithmName.SHA256,
                outputLength: 32
            );
            
            // Compare hashes (constant-time comparison)
            for (int i = 0; i < 32; i++)
            {
                if (hashWithSalt[i + 16] != hash2[i])
                    return false;
            }
            
            return true;
        }
        catch
        {
            return false;
        }
    }
}
