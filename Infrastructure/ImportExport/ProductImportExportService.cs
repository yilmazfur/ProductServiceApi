using System.Text.Json;
using WebApplication1.Application.DTOs;
using WebApplication1.Domain.Entities;
using WebApplication1.Infrastructure.Persistence.Repositories;
using WebApplication1.Infrastructure.Storage;

namespace WebApplication1.Infrastructure.ImportExport
{
    public class ProductImportExportService : IProductImportExportService
    {
        private readonly IProductRepository _repository;
        private readonly IBlobStorageService _blobStorage;
        private readonly ILogger<ProductImportExportService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public ProductImportExportService(
            IProductRepository repository,
            IBlobStorageService blobStorage,
            ILogger<ProductImportExportService> logger)
        {
            _repository = repository;
            _blobStorage = blobStorage;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };
        }

        public async Task<(Stream FileStream, string FileName)> ExportProductsAsync(bool uploadToBlob = false)
        {
            try
            {
                var products = await _repository.GetAllAsync();
                var exportProducts = products.Select(p => new ExportProductDto
                {
                    Code = p.Code,
                    Name = p.Name,
                    Category = p.Category.ToString(),
                    Content = p.Content,
                    IsActive = p.IsActive
                }).ToList();

                var jsonContent = JsonSerializer.Serialize(exportProducts, _jsonOptions);
                var fileName = $"export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";

                // Upload to blob storage if enabled
                if (uploadToBlob)
                {
                    await _blobStorage.UploadJsonAsync(fileName, jsonContent);
                }

                var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonContent));
                stream.Position = 0;

                _logger.LogInformation("Exported {Count} products to {FileName}", exportProducts.Count, fileName);

                return (stream, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during export process");
                throw new InvalidOperationException($"Export failed: {ex.Message}", ex);
            }
        }

        public async Task<List<string>> ListImportFilesAsync()
        {
            return await _blobStorage.ListFilesAsync();
        }
    }
}
