using RiskAnalyzer.Api.Data;
using RiskAnalyzer.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace RiskAnalyzer.Api.Repositories;

/// <summary>
/// Repository for risk category operations
/// Provides access to category severity weights and action thresholds stored in database
/// </summary>
public interface IRiskCategoryRepository
{
    Task<RiskCategory?> GetByCategoryNameAsync(string categoryName);
    Task<List<RiskCategory>> GetAllEnabledAsync();
    Task<List<RiskCategory>> GetAllAsync();
    Task<RiskCategory> CreateAsync(RiskCategory category);
    Task<RiskCategory> UpdateAsync(RiskCategory category);
    Task<bool> DeleteAsync(int id);
}

public class RiskCategoryRepository : IRiskCategoryRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<RiskCategoryRepository> _logger;
    private static Dictionary<string, RiskCategory>? _cache;
    private static DateTime _cacheExpiry = DateTime.MinValue;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);
    private static readonly object _lockObject = new();

    public RiskCategoryRepository(ApplicationDbContext context, ILogger<RiskCategoryRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get risk category by name with caching for performance
    /// </summary>
    public async Task<RiskCategory?> GetByCategoryNameAsync(string categoryName)
    {
        try
        {
            // Check cache first
            if (IsCacheValid() && _cache != null && _cache.TryGetValue(categoryName, out var cached))
            {
                _logger.LogDebug("Cache hit for category: {CategoryName}", categoryName);
                return cached;
            }

            // Fetch from database
            var category = await _context.RiskCategories
                .FirstOrDefaultAsync(c => c.CategoryName == categoryName);

            if (category != null)
            {
                InvalidateAndRefreshCache();
            }

            return category;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category by name: {CategoryName}", categoryName);
            throw;
        }
    }

    /// <summary>
    /// Get all enabled risk categories with caching
    /// </summary>
    public async Task<List<RiskCategory>> GetAllEnabledAsync()
    {
        try
        {
            // Check cache
            if (IsCacheValid() && _cache != null)
            {
                _logger.LogDebug("Cache hit for all enabled categories");
                return _cache.Values.Where(c => c.IsEnabled).ToList();
            }

            // Fetch from database
            var categories = await _context.RiskCategories
                .Where(c => c.IsEnabled)
                .ToListAsync();

            InvalidateAndRefreshCache();
            return categories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all enabled categories");
            throw;
        }
    }

    /// <summary>
    /// Get all risk categories (including disabled ones)
    /// </summary>
    public async Task<List<RiskCategory>> GetAllAsync()
    {
        try
        {
            var categories = await _context.RiskCategories.ToListAsync();
            return categories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all categories");
            throw;
        }
    }

    /// <summary>
    /// Create new risk category
    /// </summary>
    public async Task<RiskCategory> CreateAsync(RiskCategory category)
    {
        try
        {
            category.UpdatedAt = DateTime.UtcNow;
            _context.RiskCategories.Add(category);
            await _context.SaveChangesAsync();
            
            InvalidateCache();
            _logger.LogInformation("Risk category created: {CategoryName}", category.CategoryName);
            
            return category;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating risk category: {CategoryName}", category.CategoryName);
            throw;
        }
    }

    /// <summary>
    /// Update existing risk category
    /// </summary>
    public async Task<RiskCategory> UpdateAsync(RiskCategory category)
    {
        try
        {
            var existing = await _context.RiskCategories.FindAsync(category.Id);
            if (existing == null)
            {
                throw new InvalidOperationException($"Category with ID {category.Id} not found");
            }

            existing.SeverityWeight = category.SeverityWeight;
            existing.ActionThreshold = category.ActionThreshold;
            existing.RecommendedAction = category.RecommendedAction;
            existing.Description = category.Description;
            existing.IsEnabled = category.IsEnabled;
            existing.UpdatedAt = DateTime.UtcNow;

            _context.RiskCategories.Update(existing);
            await _context.SaveChangesAsync();
            
            InvalidateCache();
            _logger.LogInformation("Risk category updated: {CategoryName}", existing.CategoryName);
            
            return existing;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating risk category: {CategoryId}", category.Id);
            throw;
        }
    }

    /// <summary>
    /// Delete risk category
    /// </summary>
    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var category = await _context.RiskCategories.FindAsync(id);
            if (category == null)
            {
                return false;
            }

            _context.RiskCategories.Remove(category);
            await _context.SaveChangesAsync();
            
            InvalidateCache();
            _logger.LogInformation("Risk category deleted: {CategoryId}", id);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting risk category: {CategoryId}", id);
            throw;
        }
    }

    /// <summary>
    /// Check if cache is still valid
    /// </summary>
    private static bool IsCacheValid()
    {
        lock (_lockObject)
        {
            return _cache != null && DateTime.UtcNow < _cacheExpiry;
        }
    }

    /// <summary>
    /// Invalidate the cache
    /// </summary>
    private static void InvalidateCache()
    {
        lock (_lockObject)
        {
            _cache = null;
            _cacheExpiry = DateTime.MinValue;
        }
    }

    /// <summary>
    /// Refresh cache from database
    /// </summary>
    private async void InvalidateAndRefreshCache()
    {
        lock (_lockObject)
        {
            _cache = null;
            _cacheExpiry = DateTime.MinValue;
        }

        // Refresh cache
        var categories = await _context.RiskCategories.ToListAsync();
        lock (_lockObject)
        {
            _cache = categories.ToDictionary(c => c.CategoryName);
            _cacheExpiry = DateTime.UtcNow.Add(CacheDuration);
        }
    }
}
