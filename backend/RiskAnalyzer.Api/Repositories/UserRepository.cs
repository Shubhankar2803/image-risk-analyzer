namespace RiskAnalyzer.Api.Repositories;

using RiskAnalyzer.Api.Data;
using RiskAnalyzer.Api.Models;
using Microsoft.EntityFrameworkCore;

public interface IUserRepository
{
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User?> GetUserByIdAsync(Guid userId);
    Task<User> CreateUserAsync(User user);
    Task<User> UpdateUserAsync(User user);
    Task<bool> UserExistsByEmailAsync(string email);
    Task<bool> UserExistsByUsernameAsync(string username);
}

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
    
    public UserRepository(ApplicationDbContext context) => _context = context;
    
    public async Task<User?> GetUserByEmailAsync(string email) =>
        string.IsNullOrWhiteSpace(email) ? null : 
            await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    
    public async Task<User?> GetUserByUsernameAsync(string username) =>
        string.IsNullOrWhiteSpace(username) ? null :
            await _context.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
    
    public Task<User?> GetUserByIdAsync(Guid userId) =>
        _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
    
    public async Task<User> CreateUserAsync(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }
    
    public async Task<User> UpdateUserAsync(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }
    
    public Task<bool> UserExistsByEmailAsync(string email) =>
        _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower());
    
    public Task<bool> UserExistsByUsernameAsync(string username) =>
        _context.Users.AnyAsync(u => u.Username.ToLower() == username.ToLower());
}
