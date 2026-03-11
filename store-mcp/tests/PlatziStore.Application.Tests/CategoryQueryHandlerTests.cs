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
using PlatziStore.Shared.Exceptions;
using PlatziStore.Shared.Models;
using Xunit;

namespace PlatziStore.Application.Tests;

public class CategoryQueryHandlerTests
{
    private readonly Mock<IStoreGateway> _mockGateway;
    private readonly CategoryQueryHandler _handler;

    public CategoryQueryHandlerTests()
    {
        _mockGateway = new Mock<IStoreGateway>();
        _handler = new CategoryQueryHandler(_mockGateway.Object);
    }

    [Fact]
    public async Task ListCategoriesAsync_ShouldReturnMappedCategories_WhenSuccessful()
    {
        var categories = new[] { TestDataFactory.CreateProductGroup(id: 1), TestDataFactory.CreateProductGroup(id: 2) };
        _mockGateway.Setup(x => x.GetAllCategoriesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        var result = await _handler.ListCategoriesAsync();

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Should().HaveCount(2);
        result.Data![0].Id.Should().Be(1);
    }

    [Fact]
    public async Task ListCategoriesAsync_ShouldReturnFailure_WhenGatewayThrows()
    {
        _mockGateway.Setup(x => x.GetAllCategoriesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ExternalServiceException("API Error", "Details"));

        var result = await _handler.ListCategoriesAsync();

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("API Error");
    }

    [Fact]
    public async Task GetCategoryByIdAsync_ShouldReturnMappedCategory_WhenSuccessful()
    {
        var category = TestDataFactory.CreateProductGroup(id: 10);
        _mockGateway.Setup(x => x.GetCategoryByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var result = await _handler.GetCategoryByIdAsync(10);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(10);
    }

    [Fact]
    public async Task GetCategoryByIdAsync_ShouldReturnFailure_WhenNotFound()
    {
        _mockGateway.Setup(x => x.GetCategoryByIdAsync(99, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException("Category", 99));

        var result = await _handler.GetCategoryByIdAsync(99);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().ContainEquivalentOf("not found");
    }

    [Fact]
    public async Task GetCategoryBySlugAsync_ShouldReturnMappedCategory_WhenSuccessful()
    {
        var category = TestDataFactory.CreateProductGroup(slugStr: "test-slug");
        _mockGateway.Setup(x => x.GetCategoryBySlugAsync("test-slug", It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var result = await _handler.GetCategoryBySlugAsync("test-slug");

        result.IsSuccess.Should().BeTrue();
        result.Data!.Slug.Should().Be("test-slug");
    }

    [Fact]
    public async Task GetCategoryBySlugAsync_ShouldReturnFailure_WhenNotFound()
    {
        _mockGateway.Setup(x => x.GetCategoryBySlugAsync("not-found", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException("Category", "not-found"));

        var result = await _handler.GetCategoryBySlugAsync("not-found");

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().ContainEquivalentOf("not found");
    }

    [Fact]
    public async Task ListProductsByCategoryAsync_ShouldReturnMappedProducts_WhenSuccessful()
    {
        var products = new[] { TestDataFactory.CreateMerchandise(categoryId: 1) };
        _mockGateway.Setup(x => x.GetProductsByCategoryAsync(1, It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        var result = await _handler.ListProductsByCategoryAsync(1);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task ListProductsByCategoryAsync_ShouldReturnFailure_WhenCategoryNotFound()
    {
        _mockGateway.Setup(x => x.GetProductsByCategoryAsync(99, It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException("Category", 99));

        var result = await _handler.ListProductsByCategoryAsync(99);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().ContainEquivalentOf("not found");
    }
}
