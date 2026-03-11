using System;
using System.Collections.Generic;
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
using PlatziStore.Domain.ValueObjects;
using PlatziStore.Shared.Exceptions;
using Xunit;

namespace PlatziStore.Application.Tests;

public class CatalogQueryHandlerTests
{
    private readonly Mock<IStoreGateway> _mockGateway;
    private readonly CatalogQueryHandler _handler;

    public CatalogQueryHandlerTests()
    {
        _mockGateway = new Mock<IStoreGateway>();
        _handler = new CatalogQueryHandler(_mockGateway.Object);
    }

    [Fact]
    public async Task ListProductsAsync_ShouldReturnMappedProducts_WhenSuccessful()
    {
        // Arrange
        var products = new[] { TestDataFactory.CreateMerchandise(id: 1), TestDataFactory.CreateMerchandise(id: 2) };
        _mockGateway.Setup(x => x.GetAllProductsAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        // Act
        var result = await _handler.ListProductsAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(2);
        result.Data!.Items[0].Id.Should().Be(1);
    }

    [Fact]
    public async Task ListProductsAsync_ShouldReturnFailure_WhenGatewayThrows()
    {
        _mockGateway.Setup(x => x.GetAllProductsAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ExternalServiceException("API Error", "Details"));

        var result = await _handler.ListProductsAsync();

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("API Error");
    }

    [Fact]
    public async Task GetProductByIdAsync_ShouldReturnMappedProduct_WhenSuccessful()
    {
        var product = TestDataFactory.CreateMerchandise(id: 10);
        _mockGateway.Setup(x => x.GetProductByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var result = await _handler.GetProductByIdAsync(10);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(10);
    }

    [Fact]
    public async Task GetProductByIdAsync_ShouldReturnFailure_WhenNotFound()
    {
        _mockGateway.Setup(x => x.GetProductByIdAsync(99, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException("Product", 99));

        var result = await _handler.GetProductByIdAsync(99);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().ContainEquivalentOf("not found");
    }

    [Fact]
    public async Task GetProductBySlugAsync_ShouldReturnMappedProduct_WhenSuccessful()
    {
        var product = TestDataFactory.CreateMerchandise(slugStr: "test-slug");
        _mockGateway.Setup(x => x.GetProductBySlugAsync("test-slug", It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var result = await _handler.GetProductBySlugAsync("test-slug");

        result.IsSuccess.Should().BeTrue();
        result.Data!.Slug.Should().Be("test-slug");
    }

    [Fact]
    public async Task GetProductBySlugAsync_ShouldReturnFailure_WhenNotFound()
    {
        _mockGateway.Setup(x => x.GetProductBySlugAsync("not-found", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException("Product", "not-found"));

        var result = await _handler.GetProductBySlugAsync("not-found");

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task FilterProductsAsync_ShouldReturnMappedProducts_WhenSuccessful()
    {
        var products = new[] { TestDataFactory.CreateMerchandise() };
        _mockGateway.Setup(x => x.FilterProductsAsync(It.IsAny<string>(), null, null, null, null, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        var criteria = new CatalogFilterCriteria { Title = "Test" };
        var result = await _handler.FilterProductsAsync(criteria);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task FilterProductsAsync_ShouldReturnFailure_WhenGatewayThrows()
    {
        _mockGateway.Setup(x => x.FilterProductsAsync(It.IsAny<string>(), null, null, null, null, null, null, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ExternalServiceException("Error", "Details"));

        var criteria = new CatalogFilterCriteria { Title = "Test" };
        var result = await _handler.FilterProductsAsync(criteria);

        result.IsSuccess.Should().BeFalse();
    }
    
    [Fact]
    public async Task GetRelatedProductsByIdAsync_ShouldReturnMappedProducts_WhenSuccessful()
    {
        var products = new[] { TestDataFactory.CreateMerchandise(id: 2) };
        _mockGateway.Setup(x => x.GetRelatedProductsByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        var result = await _handler.GetRelatedProductsByIdAsync(1);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }
    
    [Fact]
    public async Task GetRelatedProductsByIdAsync_ShouldReturnFailure_WhenNotFound()
    {
        _mockGateway.Setup(x => x.GetRelatedProductsByIdAsync(1, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException("Product", 1));

        var result = await _handler.GetRelatedProductsByIdAsync(1);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task GetRelatedProductsBySlugAsync_ShouldReturnMappedProducts_WhenSuccessful()
    {
        var products = new[] { TestDataFactory.CreateMerchandise() };
        _mockGateway.Setup(x => x.GetRelatedProductsBySlugAsync("test-slug", It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        var result = await _handler.GetRelatedProductsBySlugAsync("test-slug");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }
    
    [Fact]
    public async Task GetRelatedProductsBySlugAsync_ShouldReturnFailure_WhenNotFound()
    {
        _mockGateway.Setup(x => x.GetRelatedProductsBySlugAsync("test-slug", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException("Product", "test-slug"));

        var result = await _handler.GetRelatedProductsBySlugAsync("test-slug");

        result.IsSuccess.Should().BeFalse();
    }
}
