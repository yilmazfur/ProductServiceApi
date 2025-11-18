using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using WebApplication1.Domain.Entities;
using WebApplication1.Infrastructure.Persistence.Data;
using WebApplication1.Infrastructure.Persistence.Repositories;

namespace ProductServiceApi.Tests;

public class SqlProductRepositoryTests
{
    private ProductDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ProductDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ProductDbContext(options);
    }

    [Fact]
    public async Task CreateAsync_Should_Set_CreatedAt_And_Persist()
    {
        using var ctx = CreateContext();
        var logger = new Mock<ILogger<SqlProductRepository>>();
        var repo = new SqlProductRepository(ctx, logger.Object);

        var product = new Product
        {
            Code = "C1",
            Name = "Prod",
            Category = ProductCategory.Food,
            IsActive = true
        };

        var created = await repo.CreateAsync(product);
        created.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(3));
        (await ctx.Products.CountAsync()).Should().Be(1);
    }

    [Fact]
    public async Task UpdateAsync_Should_Return_Null_When_NotFound()
    {
        using var ctx = CreateContext();
        var logger = new Mock<ILogger<SqlProductRepository>>();
        var repo = new SqlProductRepository(ctx, logger.Object);

        var result = await repo.UpdateAsync("NF", new Product { Code = "NF" });
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_Should_Preserve_CreatedAt()
    {
        using var ctx = CreateContext();
        var logger = new Mock<ILogger<SqlProductRepository>>();
        var repo = new SqlProductRepository(ctx, logger.Object);

        var product = new Product
        {
            Code = "U1",
            Name = "Name",
            Category = ProductCategory.Food,
            IsActive = true
        };
        await repo.CreateAsync(product);
        var originalCreated = product.CreatedAt;

        var updatedEntity = new Product
        {
            Code = "U1",
            Name = "New",
            Category = ProductCategory.NonFood,
            IsActive = false,
            CreatedAt = DateTime.UtcNow.AddYears(-2) // should be overwritten
        };

        var updated = await repo.UpdateAsync("U1", updatedEntity);
        updated.Should().NotBeNull();
        updated!.CreatedAt.Should().Be(originalCreated);
        updated.Name.Should().Be("New");
    }

    [Fact]
    public async Task DeleteAsync_Should_Remove_Product()
    {
        using var ctx = CreateContext();
        var logger = new Mock<ILogger<SqlProductRepository>>();
        var repo = new SqlProductRepository(ctx, logger.Object);

        await repo.CreateAsync(new Product { Code = "D1", Name = "Name", Category = ProductCategory.Food, IsActive = true });
        var deleted = await repo.DeleteAsync("D1");
        deleted.Should().BeTrue();
        (await ctx.Products.AnyAsync()).Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_Should_Return_True_When_Exists()
    {
        using var ctx = CreateContext();
        var logger = new Mock<ILogger<SqlProductRepository>>();
        var repo = new SqlProductRepository(ctx, logger.Object);

        await repo.CreateAsync(new Product { Code = "E1", Name = "Name", Category = ProductCategory.Food, IsActive = true });
        (await repo.ExistsAsync("E1")).Should().BeTrue();
        (await repo.ExistsAsync("NONE")).Should().BeFalse();
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_All()
    {
        using var ctx = CreateContext();
        var logger = new Mock<ILogger<SqlProductRepository>>();
        var repo = new SqlProductRepository(ctx, logger.Object);

        await repo.CreateAsync(new Product { Code = "A", Name = "A", Category = ProductCategory.Food, IsActive = true });
        await repo.CreateAsync(new Product { Code = "B", Name = "B", Category = ProductCategory.NonFood, IsActive = false });

        var all = await repo.GetAllAsync();
        all.Should().HaveCount(2);
    }
}
