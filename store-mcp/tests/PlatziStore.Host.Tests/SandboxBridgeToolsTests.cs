using Moq;
using PlatziStore.Application.Contracts;
using PlatziStore.Application.DataTransfer;
using PlatziStore.Host.Tools;
using PlatziStore.Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace PlatziStore.Host.Tests;

public class SandboxBridgeToolsTests
{
    private readonly Mock<ICatalogQueryService> _catalogServiceMock;
    private readonly Mock<ICategoryQueryService> _categoryServiceMock;
    private readonly Mock<ICustomerAccountService> _customerServiceMock;

    public SandboxBridgeToolsTests()
    {
        _catalogServiceMock = new Mock<ICatalogQueryService>();
        _categoryServiceMock = new Mock<ICategoryQueryService>();
        _customerServiceMock = new Mock<ICustomerAccountService>();
    }

    [Fact]
    public async Task AnalyzeStoreData_WithProductsScope_ReturnsStructuredJson()
    {
        // Arrange
        var products = new List<CatalogItemSummary>
        {
            new() { Id = 1, Title = "Test Product", Price = 100, Slug = "test", CategoryName = "Test" }
        };
        var pagedData = PagedCollection<CatalogItemSummary>.Create(products, 0, 10, 1);
        _catalogServiceMock.Setup(x => x.ListProductsAsync(It.IsAny<PaginationEnvelope>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(OperationOutcome<PagedCollection<CatalogItemSummary>>.Success(pagedData));

        // Act
        var result = await SandboxBridgeTools.AnalyzeStoreData(
            _catalogServiceMock.Object, 
            _categoryServiceMock.Object, 
            _customerServiceMock.Object, 
            "products");

        // Assert
        Assert.Contains("Test Product", result);
        Assert.Contains("dataScope", result);
        Assert.Contains("products", result);
        Assert.Contains("count", result);
    }

    [Fact]
    public async Task ProcessStoreExport_WithProductsScope_ReturnsNdjson()
    {
        // Arrange
        var products = new List<CatalogItemSummary>
        {
            new() { Id = 1, Title = "Test Product 1", Price = 100, Slug = "test-1", CategoryName = "Test" },
            new() { Id = 2, Title = "Test Product 2", Price = 200, Slug = "test-2", CategoryName = "Test" }
        };
        var pagedData = PagedCollection<CatalogItemSummary>.Create(products, 0, 10, 2);
        _catalogServiceMock.Setup(x => x.ListProductsAsync(It.IsAny<PaginationEnvelope>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(OperationOutcome<PagedCollection<CatalogItemSummary>>.Success(pagedData));

        // Act
        var result = await SandboxBridgeTools.ProcessStoreExport(
            _catalogServiceMock.Object, 
            _categoryServiceMock.Object, 
            _customerServiceMock.Object, 
            "products");

        // Assert
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(2, lines.Length);
        Assert.Contains("Test Product 1", lines[0]);
        Assert.Contains("Test Product 2", lines[1]);
    }

    [Fact]
    public async Task AnalyzeStoreData_WithInvalidScope_ReturnsErrorMessage()
    {
        // Act
        var result = await SandboxBridgeTools.AnalyzeStoreData(
            _catalogServiceMock.Object, 
            _categoryServiceMock.Object, 
            _customerServiceMock.Object, 
            "invalid_scope");

        // Assert
        Assert.StartsWith("Error: Invalid data scope", result);
    }

    [Fact]
    public async Task AnalyzeStoreData_WhenEmptyDataset_ReturnsClearMessage()
    {
        // Arrange
        var emptyData = PagedCollection<CatalogItemSummary>.Create(new List<CatalogItemSummary>(), 0, 10, 0);
        _catalogServiceMock.Setup(x => x.ListProductsAsync(It.IsAny<PaginationEnvelope>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(OperationOutcome<PagedCollection<CatalogItemSummary>>.Success(emptyData));

        // Act
        var result = await SandboxBridgeTools.AnalyzeStoreData(
            _catalogServiceMock.Object, 
            _categoryServiceMock.Object, 
            _customerServiceMock.Object, 
            "products");

        // Assert
        Assert.Equal("No products found.", result);
    }
}
