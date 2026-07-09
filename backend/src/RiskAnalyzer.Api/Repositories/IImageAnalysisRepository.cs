namespace RiskAnalyzer.Api.Repositories;

using RiskAnalyzer.Api.Models;

public interface IImageAnalysisRepository
{
    Task<ImageAnalysis?> GetByIdAsync(Guid id);
    Task<List<ImageAnalysis>> GetByUserIdAsync(Guid userId);
    Task AddAsync(ImageAnalysis analysis);
    Task DeleteAsync(ImageAnalysis analysis);
}
