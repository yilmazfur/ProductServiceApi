using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using WebApplication1.Domain.Entities;
using WebApplication1.Infrastructure.Persistence.Repositories;

namespace ImportExportFunction
{
    public class FunctionImport
    {
        private readonly ILogger<FunctionImport> _logger;
        private readonly IProductRepository _productRepository;

        public FunctionImport(ILogger<FunctionImport> logger, IProductRepository productRepository)
        {
            _logger = logger;
            _productRepository = productRepository;
        }

        [Function(nameof(FunctionImport))]
        public async Task Run(
            [BlobTrigger("product-svc-blob-container/{name}", Connection = "AzureWebJobsStorage")] Stream stream,
            string name)
        {
            _logger.LogInformation($"C# Blob trigger function triggered for blob: {name}");
            
            try
            {
                using var blobStreamReader = new StreamReader(stream);
                var content = await blobStreamReader.ReadToEndAsync();

                _logger.LogInformation($"Processing blob: {name}, Size: {content.Length} bytes");

                // Deserialize JSON to product list
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                var products = JsonSerializer.Deserialize<List<ImportProductDto>>(content, options);

                if (products == null || products.Count == 0)
                {
                    _logger.LogWarning("No products found in the JSON file.");
                    return;
                }

                _logger.LogInformation($"Found {products.Count} products to import");

                int successCount = 0;
                int failureCount = 0;

                // Process each product
                foreach (var productDto in products)
                {
                    try
                    {
                        // Validate required fields
                        if (string.IsNullOrWhiteSpace(productDto.Code))
                        {
                            _logger.LogWarning("Skipping product with empty code");
                            failureCount++;
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(productDto.Name))
                        {
                            _logger.LogWarning("Skipping product {Code} with empty name", productDto.Code);
                            failureCount++;
                            continue;
                        }

                        // Parse category
                        if (!Enum.TryParse<ProductCategory>(productDto.Category, true, out var category))
                        {
                            _logger.LogWarning("Invalid category '{Category}' for product {Code}", 
                                productDto.Category, productDto.Code);
                            failureCount++;
                            continue;
                        }

                        // Check if product already exists
                        var existingProduct = await _productRepository.GetByCodeAsync(productDto.Code);

                        var product = new Product
                        {
                            Code = productDto.Code,
                            Name = productDto.Name,
                            Category = category,
                            Content = productDto.Content,
                            IsActive = productDto.IsActive,
                            CreatedAt = existingProduct?.CreatedAt ?? DateTime.UtcNow
                        };

                        if (existingProduct != null)
                        {
                            // Update existing product
                            await _productRepository.UpdateAsync(productDto.Code, product);
                            _logger.LogInformation("Updated product: {Code}", productDto.Code);
                        }
                        else
                        {
                            // Create new product
                            await _productRepository.CreateAsync(product);
                            _logger.LogInformation("Created product: {Code}", productDto.Code);
                        }

                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing product {Code}", productDto.Code);
                        failureCount++;
                    }
                }

                _logger.LogInformation(
                    "Import completed for {BlobName}. Total: {Total}, Success: {Success}, Failed: {Failed}",
                    name, products.Count, successCount, failureCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process blob: {BlobName}", name);
                throw;
            }
        }
    }
    public class ImportProductDto
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Content { get; set; }
        public bool IsActive { get; set; }
    }
}
