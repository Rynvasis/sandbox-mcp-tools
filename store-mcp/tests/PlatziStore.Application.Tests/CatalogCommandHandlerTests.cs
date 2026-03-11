using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using PlatziStore.Application.Contracts;
using PlatziStore.Application.DataTransfer;
using PlatziStore.Application.Services;
using PlatziStore.Application.Tests.Helpers;
using PlatziStore.Domain.Entities;
using PlatziStore.Domain.Exceptions;
using PlatziStore.Shared.Exceptions;
using Xunit;

namespace PlatziStore.Application.Tests;

public class CatalogCommandHandlerTests
{
    private readonly Mock<IStoreGateway> _mockGateway;
    private readonly CatalogCommandHandler _handler;

    public CatalogCommandHandlerTests()
    {
        _mockGateway = new Mock<IStoreGateway>();
        _handler = new CatalogCommandHandler(_mockGateway.Object);
    }

    [Fact]
    public async Task CreateProductAsync_ShouldReturnMappedProduct_WhenSuccessful()
    {
        var payload = TestDataFactory.CreateCatalogItemPayload(title: "New Product");
        var createdProduct = TestDataFactory.CreateMerchandise(id: 1, title: "New Product");
        
        _mockGateway.Setup(x => x.CreateProductAsync(It.IsAny<CatalogItemPayload>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdProduct);

        var result = await _handler.CreateProductAsync(payload);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Title.Should().Be("New Product");
        _mockGateway.Verify(x => x.CreateProductAsync(It.Is<CatalogItemPayload>(req => req.Title == "New Product"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateProductAsync_ShouldReturnFailure_WhenTitleIsEmpty()
    {
        var payload = TestDataFactory.CreateCatalogItemPayload(title: string.Empty);

        var result = await _handler.CreateProductAsync(payload);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Title is required");
        _mockGateway.Verify(x => x.CreateProductAsync(It.IsAny<CatalogItemPayload>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateProductAsync_ShouldReturnFailure_WhenPriceIsNegative()
    {
        var payload = TestDataFactory.CreateCatalogItemPayload(price: -10m);

        var result = await _handler.CreateProductAsync(payload);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Price must be greater than zero.");
        _mockGateway.Verify(x => x.CreateProductAsync(It.IsAny<CatalogItemPayload>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateProductAsync_ShouldReturnFailure_WhenImagesEmpty()
    {
        var payload = new CatalogItemPayload 
        { 
            Title = "Title", 
            Price = 10m, 
            Description = "Description", 
            CategoryId = 1, 
            Images = Array.Empty<string>() 
        };

        var result = await _handler.CreateProductAsync(payload);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("At least one image URL is required");
        _mockGateway.Verify(x => x.CreateProductAsync(It.IsAny<CatalogItemPayload>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateProductAsync_ShouldReturnFailure_WhenGatewayThrows()
    {
        var payload = TestDataFactory.CreateCatalogItemPayload();
        _mockGateway.Setup(x => x.CreateProductAsync(It.IsAny<CatalogItemPayload>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ExternalServiceException("API Error", "Details"));

        var result = await _handler.CreateProductAsync(payload);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("API Error");
    }

    [Fact]
    public async Task UpdateProductAsync_ShouldReturnMappedProduct_WhenSuccessful()
    {
        var payload = TestDataFactory.CreateCatalogItemPayload(title: "Updated Product");
        var updatedProduct = TestDataFactory.CreateMerchandise(id: 1, title: "Updated Product");
        
        _mockGateway.Setup(x => x.UpdateProductAsync(1, It.IsAny<CatalogItemPayload>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedProduct);

        var result = await _handler.UpdateProductAsync(1, payload);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Title.Should().Be("Updated Product");
    }

    [Fact]
    public async Task UpdateProductAsync_ShouldReturnFailure_WhenValidationFails()
    {
        var payload = TestDataFactory.CreateCatalogItemPayload(title: "");

        var result = await _handler.UpdateProductAsync(1, payload);

        result.IsSuccess.Should().BeFalse();
        _mockGateway.Verify(x => x.UpdateProductAsync(It.IsAny<int>(), It.IsAny<CatalogItemPayload>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateProductAsync_ShouldReturnFailure_WhenNotFound()
    {
        var payload = TestDataFactory.CreateCatalogItemPayload();
        _mockGateway.Setup(x => x.UpdateProductAsync(99, It.IsAny<CatalogItemPayload>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException("Product", 99));

        var result = await _handler.UpdateProductAsync(99, payload);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().ContainEquivalentOf("not found");
    }

    [Fact]
    public async Task DeleteProductAsync_ShouldReturnSuccess_WhenSuccessful()
    {
        _mockGateway.Setup(x => x.DeleteProductAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.DeleteProductAsync(1);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteProductAsync_ShouldReturnFailure_WhenNotFound()
    {
        _mockGateway.Setup(x => x.DeleteProductAsync(99, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException("Product", 99));

        var result = await _handler.DeleteProductAsync(99);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().ContainEquivalentOf("not found");
    }
}
