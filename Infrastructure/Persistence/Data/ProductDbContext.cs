using Microsoft.EntityFrameworkCore;
using WebApplication1.Domain.Entities;

namespace WebApplication1.Infrastructure.Persistence.Data
{
    /// <summary>
    /// Entity Framework DbContext for Product Service
    /// </summary>
    public class ProductDbContext : DbContext
    {
        public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>(entity =>
            {
                // Table name
                entity.ToTable("Products");

                // Primary key
                entity.HasKey(p => p.Code);

                // Properties
                entity.Property(p => p.Code)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(p => p.Name)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(p => p.Category)
                    .IsRequired()
                    .HasConversion<string>(); // Store enum as string in database

                entity.Property(p => p.Content)
                    .HasMaxLength(500);

                entity.Property(p => p.IsActive)
                    .IsRequired();

                entity.Property(p => p.CreatedAt)
                    .IsRequired();

                // Indexes
                entity.HasIndex(p => p.Category);
                entity.HasIndex(p => p.IsActive);
                entity.HasIndex(p => p.CreatedAt);
            });
        }
    }
}
