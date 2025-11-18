using WebApplication1.Domain.Entities;

namespace WebApplication1.Application.DTOs
{
    public class CreateProductRequest
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public ProductCategory? Category { get; set; }   // nullable, will be required by validator
        public string? Content { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateProductRequest
    {
        public string Name { get; set; } = string.Empty;
        public ProductCategory? Category { get; set; }   // nullable, will be required by validator
        public string? Content { get; set; }
        public bool IsActive { get; set; }
    }

    public class ProductResponse
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public ProductCategory Category { get; set; }
        public string? Content { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Import/Export DTOs
    /// <summary>
    /// DTO for importing products from JSON files
    /// </summary>
    public class ImportProductDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? Content { get; set; }
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// DTO for exporting products to JSON files
    /// </summary>
    public class ExportProductDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? Content { get; set; }
        public bool IsActive { get; set; }
    }

    public class ImportResult
    {
        public int TotalRecords { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public List<ImportError> Errors { get; set; } = new();
        public string? BlobUrl { get; set; }
    }

    public class ImportError
    {
        public int LineNumber { get; set; }
        public string? ProductCode { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class ExportResult
    {
        public int TotalRecords { get; set; }
        public string? BlobUrl { get; set; }
        public DateTime ExportedAt { get; set; }
    }
}
