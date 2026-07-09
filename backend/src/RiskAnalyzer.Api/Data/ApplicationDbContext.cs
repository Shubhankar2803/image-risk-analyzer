using Microsoft.EntityFrameworkCore;
using RiskAnalyzer.Api.Models;

namespace RiskAnalyzer.Api.Data;

/// <summary>
/// Entity Framework Core DbContext for Risk Analyzer application
/// Manages all database interactions and entity mappings
/// </summary>
public class ApplicationDbContext : DbContext
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="options">Database context options</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }
    
    /// <summary>DbSet for Users table</summary>
    public DbSet<User> Users { get; set; } = null!;
    
    /// <summary>DbSet for ImageAnalyses table</summary>
    public DbSet<ImageAnalysis> ImageAnalyses { get; set; } = null!;
    
    /// <summary>DbSet for AnalysisTags table</summary>
    public DbSet<AnalysisTag> AnalysisTags { get; set; } = null!;
    
    
    /// <summary>
    /// Configure entity relationships and constraints
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(256);
            
            entity.Property(e => e.Username)
                .IsRequired()
                .HasMaxLength(128);
            
            entity.Property(e => e.PasswordHash)
                .IsRequired();
            
            entity.Property(e => e.FullName)
                .HasMaxLength(256);
            
            // Email and Username must be unique
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Username).IsUnique();
            
            // Cascade delete: when user is deleted, their analyses are deleted
            entity.HasMany(e => e.ImageAnalyses)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // ImageAnalysis configuration
        modelBuilder.Entity<ImageAnalysis>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.FileName)
                .IsRequired()
                .HasMaxLength(500);
            
            entity.Property(e => e.FilePath)
                .IsRequired()
                .HasMaxLength(1000);
            
            entity.Property(e => e.ContentType)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.Classification)
                .IsRequired()
                .HasMaxLength(50);
            
            entity.Property(e => e.AnalysisDetails)
                .IsRequired();
            
            // Foreign key
            entity.HasOne(e => e.User)
                .WithMany(e => e.ImageAnalyses)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Cascade delete: when analysis is deleted, tags are deleted
            entity.HasMany(e => e.Tags)
                .WithOne(e => e.ImageAnalysis)
                .HasForeignKey(e => e.ImageAnalysisId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Indexes for performance
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CreatedAt).IsDescending();
            entity.HasIndex(e => e.Classification);
        });
        
        // AnalysisTag configuration
        modelBuilder.Entity<AnalysisTag>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.Category)
                .IsRequired()
                .HasMaxLength(50);
            
            // Foreign key
            entity.HasOne(e => e.ImageAnalysis)
                .WithMany(e => e.Tags)
                .HasForeignKey(e => e.ImageAnalysisId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Index for performance
            entity.HasIndex(e => e.ImageAnalysisId);
        });

        // (Refresh tokens removed)
    }
}
