using System.Text.Json;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebApplication1.Domain.Entities;
using WebApplication1.Infrastructure.Persistence.Repositories;
using WebApplication1.Application.DTOs;

namespace ImportExportFunction
{
    /// <summary>
    /// Azure Function that exports all products from the database to blob storage
    /// </summary>
    public class FunctionExport
    {
        private readonly ILogger<FunctionExport> _logger;
        private readonly IProductRepository _productRepository;
        private readonly IConfiguration _configuration;

        public FunctionExport(
            ILogger<FunctionExport> logger, 
            IProductRepository productRepository,
            IConfiguration configuration)
        {
            _logger = logger;
            _productRepository = productRepository;
            _configuration = configuration;
        }

        /// <summary>
        /// Scheduled export function - runs every 5 minutes
        /// Timer format: {second} {minute} {hour} {day} {month} {day-of-week}
        /// "0 */5 * * * *" = Every 5 minutes
        /// Change to "0 0 0 * * *" for daily at midnight (production)
        /// 0 0 * * * * -> hourly
        /// </summary>
        [Function("FunctionExportTimer")]
        public async Task RunTimer([TimerTrigger("0 0 * * * *")] TimerInfo timerInfo)
        {
            _logger.LogInformation($"[TIMER] Export function triggered at: {DateTime.UtcNow}");
            await ExportProductsAsync();
        }

        /// <summary>
        /// HTTP-triggered export function for manual testing
        /// Call: GET http://localhost:7071/api/export or POST
        /// </summary>
        [Function("FunctionExportHttp")]
        public async Task<IActionResult> RunHttp(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation($"[HTTP] Manual export triggered at: {DateTime.UtcNow}");
            
            try
            {
                var result = await ExportProductsAsync();
                return new OkObjectResult(new
                {
                    success = true,
                    message = $"Exported {result.productCount} products",
                    fileName = result.fileName,
                    blobUrl = result.blobUrl,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HTTP export failed");
                return new ObjectResult(new
                {
                    success = false,
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                })
                {
                    StatusCode = 500
                };
            }
        }

        /// <summary>
        /// Core export logic shared by both triggers
        /// </summary>
        private async Task<(int productCount, string fileName, string blobUrl)> ExportProductsAsync()
        {
            try
            {
                // Get all products from database
                _logger.LogInformation("Fetching products from database...");
                var products = await _productRepository.GetAllAsync();
                
                if (products == null || !products.Any())
                {
                    _logger.LogWarning("?? No products found in database to export");
                    return (0, string.Empty, string.Empty);
                }

                var productCount = products.Count();
                _logger.LogInformation($"?? Found {productCount} products to export");

                // Convert to export DTOs
                var exportProducts = products.Select(p => new ExportProductDto
                {
                    Code = p.Code,
                    Name = p.Name,
                    Category = p.Category.ToString(),
                    Content = p.Content,
                    IsActive = p.IsActive
                }).ToList();

                // Serialize to JSON
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true
                };

                var jsonContent = JsonSerializer.Serialize(exportProducts, jsonOptions);
                var fileName = $"exports/export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";

                _logger.LogInformation($"?? Uploading to blob storage: {fileName}");

                // Upload to blob storage
                var blobUrl = await UploadToBlobAsync(fileName, jsonContent);

                _logger.LogInformation(
                    $"? Successfully exported {productCount} products to {fileName}");
                _logger.LogInformation($"?? Blob URL: {blobUrl}");

                return (productCount, fileName, blobUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? Failed to export products");
                throw;
            }
        }

        /// <summary>
        /// Upload JSON content to Azure Blob Storage
        /// </summary>
        private async Task<string> UploadToBlobAsync(string fileName, string jsonContent)
        {
            try
            {
                var connectionString = _configuration["AzureWebJobsStorage"];
                
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new InvalidOperationException("AzureWebJobsStorage connection string is not configured");
                }

                _logger.LogInformation($"?? Connecting to Azure Blob Storage...");
                var blobServiceClient = new BlobServiceClient(connectionString);
                var containerName = _configuration["ExportContainerName"] ?? "product-svc-blob-container";
                
                _logger.LogInformation($"?? Container: {containerName}");
                var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

                // Create container if it doesn't exist
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

                var blobClient = containerClient.GetBlobClient(fileName);

                // Upload the JSON content
                using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonContent));
                await blobClient.UploadAsync(stream, overwrite: true);

                _logger.LogInformation($"? File uploaded successfully: {fileName}");
                
                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"? Error uploading file to blob storage: {fileName}");
                throw new InvalidOperationException($"Failed to upload file to blob storage: {ex.Message}", ex);
            }
        }
    }
}
