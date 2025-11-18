using WebApplication1.Application.DTOs;
using WebApplication1.Domain.Entities;

namespace WebApplication1.Application.Mappings
{
    /// <summary>
    /// Extension methods for Product entity and DTOs mapping
    /// </summary>
    public static class ProductExtensions
    {
        /// <summary>
        /// Convert Product entity to ProductResponse DTO
        /// </summary>
        public static ProductResponse ToResponse(this Product product)
        {
            return new ProductResponse
            {
                Code = product.Code,
                Name = product.Name,
                Category = product.Category,
                Content = product.Content,
                IsActive = product.IsActive,
                CreatedAt = product.CreatedAt
            };
        }

        /// <summary>
        /// Convert collection of Products to ProductResponse collection
        /// </summary>
        public static IEnumerable<ProductResponse> ToResponse(this IEnumerable<Product> products)
        {
            return products.Select(p => p.ToResponse());
        }

        /// <summary>
        /// Convert CreateProductRequest to Product entity
        /// </summary>
        public static Product ToEntity(this CreateProductRequest request)
        {
            return new Product
            {
                Code = request.Code,
                Name = request.Name,
                Category = request.Category!.Value, // validated as not null
                Content = request.Content,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Update existing Product entity from UpdateProductRequest
        /// </summary>
        public static Product UpdateFrom(this Product product, UpdateProductRequest request)
        {
            product.Name = request.Name;
            product.Category = request.Category!.Value; // validated as not null
            product.Content = request.Content;
            product.IsActive = request.IsActive;
            return product;
        }
    }
}
