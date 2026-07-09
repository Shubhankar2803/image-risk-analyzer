namespace RiskAnalyzer.Api.Repositories;

using RiskAnalyzer.Api.Data;
using RiskAnalyzer.Api.Models;
using Microsoft.EntityFrameworkCore;

public class ImageAnalysisRepository : IImageAnalysisRepository
{
    private readonly ApplicationDbContext _db;

    public ImageAnalysisRepository(ApplicationDbContext db) => _db = db;

    public async Task<ImageAnalysis?> GetByIdAsync(Guid id) =>
        await _db.ImageAnalyses.Include(a => a.Tags).FirstOrDefaultAsync(a => a.Id == id);

    public async Task<List<ImageAnalysis>> GetByUserIdAsync(Guid userId) =>
        await _db.ImageAnalyses.Where(a => a.UserId == userId).Include(a => a.Tags).OrderByDescending(a => a.CreatedAt).ToListAsync();

    public async Task AddAsync(ImageAnalysis analysis)
    {
        _db.ImageAnalyses.Add(analysis);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(ImageAnalysis analysis)
    {
        _db.ImageAnalyses.Remove(analysis);
        await _db.SaveChangesAsync();
    }
}
