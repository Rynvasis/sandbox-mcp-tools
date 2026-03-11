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

public class CategoryCommandHandlerTests
{
    private readonly Mock<IStoreGateway> _mockGateway;
    private readonly CategoryCommandHandler _handler;

    public CategoryCommandHandlerTests()
    {
        _mockGateway = new Mock<IStoreGateway>();
        _handler = new CategoryCommandHandler(_mockGateway.Object);
    }

    [Fact]
    public async Task CreateCategoryAsync_ShouldReturnMappedCategory_WhenSuccessful()
    {
        var payload = TestDataFactory.CreateCategoryPayload("New Category");
        var createdCategory = TestDataFactory.CreateProductGroup(id: 1, name: "New Category");
        
        _mockGateway.Setup(x => x.CreateCategoryAsync(It.IsAny<CategoryPayload>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdCategory);

        var result = await _handler.CreateCategoryAsync(payload);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Name.Should().Be("New Category");
        _mockGateway.Verify(x => x.CreateCategoryAsync(It.Is<CategoryPayload>(req => req.Name == "New Category"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateCategoryAsync_ShouldReturnFailure_WhenNameIsEmpty()
    {
        var payload = new CategoryPayload { Name = "", Image = "https://example.com/img.jpg" };

        var result = await _handler.CreateCategoryAsync(payload);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Name is required");
        _mockGateway.Verify(x => x.CreateCategoryAsync(It.IsAny<CategoryPayload>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateCategoryAsync_ShouldReturnFailure_WhenImageIsEmpty()
    {
        var payload = new CategoryPayload { Name = "Valid", Image = "" };

        var result = await _handler.CreateCategoryAsync(payload);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Image URL is required");
        _mockGateway.Verify(x => x.CreateCategoryAsync(It.IsAny<CategoryPayload>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateCategoryAsync_ShouldReturnFailure_WhenGatewayThrows()
    {
        var payload = TestDataFactory.CreateCategoryPayload();
        _mockGateway.Setup(x => x.CreateCategoryAsync(It.IsAny<CategoryPayload>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ExternalServiceException("API Error", "Details"));

        var result = await _handler.CreateCategoryAsync(payload);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("API Error");
    }

    [Fact]
    public async Task UpdateCategoryAsync_ShouldReturnMappedCategory_WhenSuccessful()
    {
        var payload = TestDataFactory.CreateCategoryPayload("Updated Category");
        var updatedCategory = TestDataFactory.CreateProductGroup(id: 1, name: "Updated Category");
        
        _mockGateway.Setup(x => x.UpdateCategoryAsync(1, It.IsAny<CategoryPayload>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedCategory);

        var result = await _handler.UpdateCategoryAsync(1, payload);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Name.Should().Be("Updated Category");
    }

    [Fact]
    public async Task UpdateCategoryAsync_ShouldReturnFailure_WhenValidationFails()
    {
        var payload = new CategoryPayload { Name = "", Image = "img" };

        var result = await _handler.UpdateCategoryAsync(1, payload);

        result.IsSuccess.Should().BeFalse();
        _mockGateway.Verify(x => x.UpdateCategoryAsync(It.IsAny<int>(), It.IsAny<CategoryPayload>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateCategoryAsync_ShouldReturnFailure_WhenNotFound()
    {
        var payload = TestDataFactory.CreateCategoryPayload();
        _mockGateway.Setup(x => x.UpdateCategoryAsync(99, It.IsAny<CategoryPayload>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException("Category", 99));

        var result = await _handler.UpdateCategoryAsync(99, payload);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().ContainEquivalentOf("not found");
    }

    [Fact]
    public async Task DeleteCategoryAsync_ShouldReturnSuccess_WhenSuccessful()
    {
        _mockGateway.Setup(x => x.DeleteCategoryAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.DeleteCategoryAsync(1);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteCategoryAsync_ShouldReturnFailure_WhenIdIsInvalid()
    {
        var result = await _handler.DeleteCategoryAsync(0);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Invalid category ID");
        _mockGateway.Verify(x => x.DeleteCategoryAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteCategoryAsync_ShouldReturnFailure_WhenNotFound()
    {
        _mockGateway.Setup(x => x.DeleteCategoryAsync(99, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException("Category", 99));

        var result = await _handler.DeleteCategoryAsync(99);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().ContainEquivalentOf("not found");
    }
}
