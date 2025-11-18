using FluentAssertions;
using Moq;
using WebApplication1.Application.Features.Products.Commands;
using WebApplication1.Application.Features.Products.Queries;
using WebApplication1.Application.DTOs;
using WebApplication1.Domain.Entities;
using WebApplication1.Infrastructure.Persistence.Repositories;

namespace ProductServiceApi.Tests;

public class ProductCommandQueryTests
{
    [Fact]
    public async Task CreateProductCommand_Should_Create_Product()
    {
        var repo = new Mock<IProductRepository>();
        repo.Setup(r => r.ExistsAsync("CODE1")).ReturnsAsync(false);
        repo.Setup(r => r.CreateAsync(It.IsAny<Product>()))
            .ReturnsAsync((Product p) => p);

        var handler = new CreateProductCommandHandler(repo.Object);
        var cmd = new CreateProductCommand(new CreateProductRequest
        {
            Code = "CODE1",
            Name = "Test Product",
            Category = ProductCategory.Food,
            Content = "Desc",
            IsActive = true
        });

        var response = await handler.Handle(cmd, CancellationToken.None);
        response.Code.Should().Be("CODE1");
        response.Name.Should().Be("Test Product");
        response.Category.Should().Be(ProductCategory.Food);
        repo.Verify(r => r.CreateAsync(It.Is<Product>(p => p.Code == "CODE1")), Times.Once);
    }

    [Fact]
    public async Task CreateProductCommand_Should_Throw_When_Code_Exists()
    {
        var repo = new Mock<IProductRepository>();
        repo.Setup(r => r.ExistsAsync("DUPL")).ReturnsAsync(true);
        var handler = new CreateProductCommandHandler(repo.Object);

        var cmd = new CreateProductCommand(new CreateProductRequest
        {
            Code = "DUPL",
            Name = "Dup Product",
            Category = ProductCategory.Food
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(cmd, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateProductCommand_Should_Update_Product()
    {
        var existing = new Product
        {
            Code = "UPD1",
            Name = "Old Name",
            Category = ProductCategory.Food,
            Content = "Old",
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var repo = new Mock<IProductRepository>();
        repo.Setup(r => r.GetByCodeAsync("UPD1")).ReturnsAsync(existing);
        repo.Setup(r => r.UpdateAsync("UPD1", It.IsAny<Product>()))
            .ReturnsAsync((string c, Product p) => p);

        var handler = new UpdateProductCommandHandler(repo.Object);
        var cmd = new UpdateProductCommand("UPD1", new UpdateProductRequest
        {
            Name = "New Name",
            Category = ProductCategory.NonFood,
            Content = "New",
            IsActive = false
        });

        var response = await handler.Handle(cmd, CancellationToken.None);
        response.Name.Should().Be("New Name");
        response.Category.Should().Be(ProductCategory.NonFood);
        response.IsActive.Should().BeFalse();
        repo.Verify(r => r.UpdateAsync("UPD1", It.Is<Product>(p => p.Name == "New Name")), Times.Once);
    }

    [Fact]
    public async Task UpdateProductCommand_Should_Throw_When_NotFound()
    {
        var repo = new Mock<IProductRepository>();
        repo.Setup(r => r.GetByCodeAsync("NF")).ReturnsAsync((Product?)null);

        var handler = new UpdateProductCommandHandler(repo.Object);
        var cmd = new UpdateProductCommand("NF", new UpdateProductRequest
        {
            Name = "Name",
            Category = ProductCategory.Food
        });

        await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(cmd, CancellationToken.None));
    }

    [Fact]
    public async Task DeleteProductCommand_Should_Return_True_On_Delete()
    {
        var repo = new Mock<IProductRepository>();
        repo.Setup(r => r.DeleteAsync("DEL1")).ReturnsAsync(true);
        var handler = new DeleteProductCommandHandler(repo.Object);

        var result = await handler.Handle(new DeleteProductCommand("DEL1"), CancellationToken.None);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task GetProductByCodeQuery_Should_Return_Product()
    {
        var product = new Product
        {
            Code = "GET1",
            Name = "Get Name",
            Category = ProductCategory.Food,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        var repo = new Mock<IProductRepository>();
        repo.Setup(r => r.GetByCodeAsync("GET1")).ReturnsAsync(product);

        var handler = new GetProductByCodeQueryHandler(repo.Object);
        var response = await handler.Handle(new GetProductByCodeQuery("GET1"), CancellationToken.None);
        response.Should().NotBeNull();
        response!.Code.Should().Be("GET1");
    }

    [Fact]
    public async Task GetAllProductsQuery_Should_Return_List()
    {
        var products = new List<Product>
        {
            new Product { Code = "A", Name = "A", Category = ProductCategory.Food, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Product { Code = "B", Name = "B", Category = ProductCategory.NonFood, IsActive = false, CreatedAt = DateTime.UtcNow }
        };
        var repo = new Mock<IProductRepository>();
        repo.Setup(r => r.GetAllAsync()).ReturnsAsync(products);

        var handler = new GetAllProductsQueryHandler(repo.Object);
        var response = await handler.Handle(new GetAllProductsQuery(), CancellationToken.None);
        response.Should().HaveCount(2);
        response.Select(r => r.Code).Should().Contain(new[] { "A", "B" });
    }
}
