using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using PlatziStore.Application.Contracts;
using PlatziStore.Application.DataTransfer;
using PlatziStore.Host.Tools;
using PlatziStore.Infrastructure.Configuration;
using PlatziStore.Infrastructure.Observability;
using PlatziStore.Shared.Models;
using Xunit;

namespace PlatziStore.Host.Tests;

public class CatalogBrowsingToolsTests
{
    private readonly Mock<ICatalogQueryService> _mockService;
    private readonly StructuredEventLogger _logger;

    public CatalogBrowsingToolsTests()
    {
        _mockService = new Mock<ICatalogQueryService>();
        
        var metrics = new ToolInvocationMetrics();
        var options = Options.Create(new TelemetryOptions { MetricsEnabled = false });
        var loggerMock = new Mock<ILogger<StructuredEventLogger>>();
        _logger = new StructuredEventLogger(loggerMock.Object, options, metrics);
    }

    [Fact]
    public async Task ListStoreProducts_ReturnsFormattedList_WhenSuccessful()
    {
        var items = new List<CatalogItemSummary>
        {
            new() { Id = 1, Title = "Product A", Price = 10 }
        };
        var outcome = OperationOutcome<PagedCollection<CatalogItemSummary>>.Success(
            PagedCollection<CatalogItemSummary>.Create(items, 0, 10));
            
        _mockService.Setup(x => x.ListProductsAsync(It.IsAny<PaginationEnvelope>(), default))
            .ReturnsAsync(outcome);

        var result = await CatalogBrowsingTools.ListStoreProducts(_mockService.Object, _logger);

        result.Should().Contain("Product A");
        result.Should().Contain("$10");
    }

    [Fact]
    public async Task ListStoreProducts_ReturnsNoProductsMessage_WhenEmpty()
    {
        var outcome = OperationOutcome<PagedCollection<CatalogItemSummary>>.Success(
            PagedCollection<CatalogItemSummary>.Create(new List<CatalogItemSummary>(), 0, 10));
            
        _mockService.Setup(x => x.ListProductsAsync(It.IsAny<PaginationEnvelope>(), default))
            .ReturnsAsync(outcome);

        var result = await CatalogBrowsingTools.ListStoreProducts(_mockService.Object, _logger);

        result.Should().Contain("No products found");
    }

    [Fact]
    public async Task GetProductById_ReturnsFormattedDetail_WhenSuccessful()
    {
        var item = new CatalogItemDetail { Id = 1, Title = "Product A", Description = "Desc A", Price = 10m };
        var outcome = OperationOutcome<CatalogItemDetail>.Success(item);
            
        _mockService.Setup(x => x.GetProductByIdAsync(1, default))
            .ReturnsAsync(outcome);

        var result = await CatalogBrowsingTools.GetProductById(_mockService.Object, _logger, 1);

        result.Should().Contain("Product A");
        result.Should().Contain("Desc A");
    }

    [Fact]
    public async Task GetProductBySlug_ReturnsFormattedDetail_WhenSuccessful()
    {
        var item = new CatalogItemDetail { Id = 1, Title = "Product Slug", Slug = "prod-slug" };
        var outcome = OperationOutcome<CatalogItemDetail>.Success(item);
            
        _mockService.Setup(x => x.GetProductBySlugAsync("prod-slug", default))
            .ReturnsAsync(outcome);

        var result = await CatalogBrowsingTools.GetProductBySlug(_mockService.Object, _logger, "prod-slug");

        result.Should().Contain("Product Slug");
        result.Should().Contain("prod-slug");
    }

    [Fact]
    public async Task FindRelatedProducts_ReturnsFormattedRelated_WhenSuccessful()
    {
        var items = new List<CatalogItemSummary> { new() { Id = 2, Title = "Related B" } };
        var outcome = OperationOutcome<IReadOnlyList<CatalogItemSummary>>.Success(items);
            
        _mockService.Setup(x => x.GetRelatedProductsByIdAsync(1, default))
            .ReturnsAsync(outcome);

        var result = await CatalogBrowsingTools.FindRelatedProducts(_mockService.Object, _logger, 1);

        result.Should().Contain("Found 1 related");
        result.Should().Contain("Related B");
    }

    [Fact]
    public async Task FindRelatedBySlug_ReturnsFormattedRelated_WhenSuccessful()
    {
        var items = new List<CatalogItemSummary> { new() { Id = 3, Title = "Related C" } };
        var outcome = OperationOutcome<IReadOnlyList<CatalogItemSummary>>.Success(items);
            
        _mockService.Setup(x => x.GetRelatedProductsBySlugAsync("slug-test", default))
            .ReturnsAsync(outcome);

        var result = await CatalogBrowsingTools.FindRelatedBySlug(_mockService.Object, _logger, "slug-test");

        result.Should().Contain("Related C");
    }

    [Fact]
    public async Task FilterStoreProducts_ReturnsFormattedList_WhenSuccessful()
    {
        var items = new List<CatalogItemSummary> { new() { Id = 4, Title = "Filtered D" } };
        var outcome = OperationOutcome<PagedCollection<CatalogItemSummary>>.Success(
            PagedCollection<CatalogItemSummary>.Create(items, 0, 10));
            
        _mockService.Setup(x => x.FilterProductsAsync(It.IsAny<CatalogFilterCriteria>(), default))
            .ReturnsAsync(outcome);

        var result = await CatalogBrowsingTools.FilterStoreProducts(_mockService.Object, _logger, title: "Filtered");

        result.Should().Contain("Filtered D");
    }
}
