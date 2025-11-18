using FluentValidation;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Infrastructure.Persistence.Data;
using WebApplication1.Presentation.WebApi.Middleware;
using WebApplication1.Infrastructure.Persistence.Repositories;
using WebApplication1.Application.Behaviors;
using WebApplication1.Infrastructure.Storage;
using WebApplication1.Infrastructure.ImportExport;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// Swagger / OpenAPI configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add Database Context
builder.Services.AddDbContext<ProductDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("ProductDatabase");
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    });
});

// Add MediatR with ValidationBehavior pipeline
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

// Add FluentValidation - automatically registers all validators in the assembly
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Add Repository - Using SQL Server
builder.Services.AddScoped<IProductRepository, SqlProductRepository>();

// Add Blob Storage Service
builder.Services.AddSingleton<IBlobStorageService, AzureBlobStorageService>();

// Add Import/Export Service
builder.Services.AddScoped<IProductImportExportService, ProductImportExportService>();

var app = builder.Build();

// Apply database migrations automatically on startup (only if migrations exist)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        var context = services.GetRequiredService<ProductDbContext>();
        
        // Check if there are pending migrations
        var pendingMigrations = context.Database.GetPendingMigrations();
        
        if (pendingMigrations.Any())
        {
            logger.LogInformation("Applying {Count} pending database migrations...", pendingMigrations.Count());
            context.Database.Migrate();
            logger.LogInformation("Database migrations applied successfully");
        }
        else
        {
            logger.LogInformation("Database is up to date. No pending migrations.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while checking/applying database migrations");
        logger.LogWarning("Application will continue without database migrations. You may need to run migrations manually.");
        logger.LogWarning("Run: dotnet ef migrations add InitialCreate && dotnet ef database update");
    }
}

app.UseCors("AllowAll");

// Global exception handler should be early in the pipeline
app.UseGlobalExceptionHandler();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
