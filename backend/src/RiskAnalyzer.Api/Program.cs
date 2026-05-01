using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using RiskAnalyzer.Api.Config;
using RiskAnalyzer.Api.Data;
using RiskAnalyzer.Api.Repositories;
using RiskAnalyzer.Api.Services;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

// ==================== CONFIGURATION BINDINGS ====================
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<GeminiSettings>(builder.Configuration.GetSection("GeminiSettings"));
builder.Services.Configure<MLSettings>(builder.Configuration.GetSection("MLSettings"));
builder.Services.Configure<FileUploadSettings>(builder.Configuration.GetSection("FileUpload"));
builder.Services.Configure<CorsSettings>(builder.Configuration.GetSection("Cors"));

// Register GeminiSettings as singleton for direct injection (kept for backward compatibility)
var geminiSettings = builder.Configuration.GetSection("GeminiSettings").Get<GeminiSettings>();
if (geminiSettings != null)
{
    builder.Services.AddSingleton(geminiSettings);
}

// Register MLSettings as singleton for direct injection
var mlSettings = builder.Configuration.GetSection("MLSettings").Get<MLSettings>();
if (mlSettings != null)
{
    builder.Services.AddSingleton(mlSettings);
}

// Add HttpClient factory
builder.Services.AddHttpClient();

// ==================== DATABASE SETUP ====================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    )
);

// ==================== DEPENDENCY INJECTION ====================
// Register repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRiskCategoryRepository, RiskCategoryRepository>();

// Register ML services (singleton because model loading is expensive)
builder.Services.AddSingleton<IMLModelService, MLModelService>();

// Register services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IRiskScoringService, RiskScoringService>();
builder.Services.AddScoped<IImageAnalysisService, ImageAnalysisService>();

// Register controllers
builder.Services.AddControllers();

// ==================== JWT AUTHENTICATION ====================
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
if (jwtSettings != null)
{
    var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);
    
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });
}

// ==================== CORS SETUP ====================
var corsSettings = builder.Configuration.GetSection("Cors").Get<CorsSettings>();
if (corsSettings != null)
{
    var allowedOrigins = corsSettings.AllowedOrigins.Split(',').Select(o => o.Trim()).ToArray();
    var allowedMethods = corsSettings.AllowedMethods.Split(',').Select(m => m.Trim()).ToArray();
    var allowedHeaders = corsSettings.AllowedHeaders.Split(',').Select(h => h.Trim()).ToArray();
    
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowConfiguredOrigins", policy =>
        {
            policy.WithOrigins(allowedOrigins)
                .WithMethods(allowedMethods)
                .WithHeaders(allowedHeaders)
                .AllowCredentials();
        });
    });
}

// ==================== BUILD AND CONFIGURE APP ====================
var app = builder.Build();

// Configure the HTTP request pipeline
app.UseHttpsRedirection();
app.UseCors("AllowConfiguredOrigins");
app.UseAuthentication();
app.UseAuthorization();

// ==================== INITIALIZE ML MODEL ====================
using (var scope = app.Services.CreateScope())
{
    var mlModelService = scope.ServiceProvider.GetRequiredService<IMLModelService>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("Initializing ML.NET model...");
        await mlModelService.InitializeAsync();
        logger.LogInformation("ML.NET model initialized successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to initialize ML.NET model. The application will attempt to load or train the model on first use.");
    }
}

// ==================== HEALTH CHECK ====================
app.MapGet("/health", () => Results.Ok(new { status = "API is running", timestamp = DateTime.UtcNow }));

// Map all controllers
app.MapControllers();

// ==================== RUN APP ====================
app.Run();
