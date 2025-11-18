using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Infrastructure.Persistence.Data;
using WebApplication1.Infrastructure.Persistence.Repositories;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Add CORS support for all origins (training project)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

//// Get connection string from configuration
//var connectionString = builder.Configuration.GetValue<string>("ProductDatabase") 
//    ?? "Server=tcp:products-service-server.database.windows.net,1433;Initial Catalog=products-db;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication=Active Directory Default;";

var connectionString = builder.Configuration.GetValue<string>("ProductDatabase")
    ?? "Server=tcp:products-service-server.database.windows.net,1433;Initial Catalog=products-db;User ID=SqlDbAdmin;Password=Wasd.123;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication=SqlPassword;";

// Register DbContext
builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register Repository
builder.Services.AddScoped<IProductRepository, SqlProductRepository>();

// Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
// builder.Services
//     .AddApplicationInsightsTelemetryWorkerService()
//     .ConfigureFunctionsApplicationInsights();

builder.Build().Run();
