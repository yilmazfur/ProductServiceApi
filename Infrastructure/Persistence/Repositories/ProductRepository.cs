using Microsoft.EntityFrameworkCore;
using WebApplication1.Infrastructure.Persistence.Data;
using WebApplication1.Domain.Entities;

namespace WebApplication1.Infrastructure.Persistence.Repositories
{
    public interface IProductRepository
    {
        Task<Product?> GetByCodeAsync(string code);
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product> CreateAsync(Product product);
        Task<Product?> UpdateAsync(string code, Product product);
        Task<bool> DeleteAsync(string code);
        Task<bool> ExistsAsync(string code);
    }

    /// <summary>
    /// SQL Server implementation of Product Repository using Entity Framework Core
    /// </summary>
    public class SqlProductRepository : IProductRepository
    {
        private readonly ProductDbContext _context;
        private readonly ILogger<SqlProductRepository> _logger;

        public SqlProductRepository(ProductDbContext context, ILogger<SqlProductRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Product?> GetByCodeAsync(string code)
        {
            return await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Code == code);
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Product> CreateAsync(Product product)
        {
            product.CreatedAt = DateTime.UtcNow;
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Created product with code: {Code}", product.Code);
            return product;
        }

        public async Task<Product?> UpdateAsync(string code, Product product)
        {
            var existing = await _context.Products.FindAsync(code);
            if (existing == null)
            {
                return null;
            }

            // Preserve original CreatedAt
            product.Code = code;
            product.CreatedAt = existing.CreatedAt;

            _context.Entry(existing).State = EntityState.Detached;
            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated product with code: {Code}", code);
            return product;
        }

        public async Task<bool> DeleteAsync(string code)
        {
            var product = await _context.Products.FindAsync(code);
            if (product == null)
            {
                return false;
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted product with code: {Code}", code);
            return true;
        }

        public async Task<bool> ExistsAsync(string code)
        {
            return await _context.Products.AnyAsync(p => p.Code == code);
        }
    }

}
