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

public class CategoryBrowsingToolsTests
{
    private readonly Mock<ICategoryQueryService> _mockService;
    private readonly StructuredEventLogger _logger;

    public CategoryBrowsingToolsTests()
    {
        _mockService = new Mock<ICategoryQueryService>();
        
        var metrics = new ToolInvocationMetrics();
        var options = Options.Create(new TelemetryOptions { MetricsEnabled = false });
        var loggerMock = new Mock<ILogger<StructuredEventLogger>>();
        _logger = new StructuredEventLogger(loggerMock.Object, options, metrics);
    }

    [Fact]
    public async Task ListStoreCategories_ReturnsFormattedList_WhenSuccessful()
    {
        var items = new List<CategorySummary> { new() { Id = 1, Name = "Cat A" } };
        var outcome = OperationOutcome<IReadOnlyList<CategorySummary>>.Success(items);
            
        _mockService.Setup(x => x.ListCategoriesAsync(default))
            .ReturnsAsync(outcome);

        var result = await CategoryBrowsingTools.ListStoreCategories(_mockService.Object, _logger);

        result.Should().Contain("Cat A");
    }

    [Fact]
    public async Task GetCategoryById_ReturnsFormattedDetail_WhenSuccessful()
    {
        var item = new CategorySummary { Id = 1, Name = "Cat A" };
        var outcome = OperationOutcome<CategorySummary>.Success(item);
            
        _mockService.Setup(x => x.GetCategoryByIdAsync(1, default))
            .ReturnsAsync(outcome);

        var result = await CategoryBrowsingTools.GetCategoryById(_mockService.Object, _logger, 1);

        result.Should().Contain("Cat A");
    }

    [Fact]
    public async Task GetCategoryBySlug_ReturnsFormattedDetail_WhenSuccessful()
    {
        var item = new CategorySummary { Id = 1, Name = "Cat Slug", Slug = "cat-slug" };
        var outcome = OperationOutcome<CategorySummary>.Success(item);
            
        _mockService.Setup(x => x.GetCategoryBySlugAsync("cat-slug", default))
            .ReturnsAsync(outcome);

        var result = await CategoryBrowsingTools.GetCategoryBySlug(_mockService.Object, _logger, "cat-slug");

        result.Should().Contain("Cat Slug");
    }

    [Fact]
    public async Task ListProductsInCategory_ReturnsFormattedList_WhenSuccessful()
    {
        var items = new List<CatalogItemSummary> { new() { Id = 1, Title = "Prod in Cat" } };
        var outcome = OperationOutcome<PagedCollection<CatalogItemSummary>>.Success(
            PagedCollection<CatalogItemSummary>.Create(items, 0, 10));
            
        _mockService.Setup(x => x.ListProductsByCategoryAsync(1, It.IsAny<PaginationEnvelope>(), default))
            .ReturnsAsync(outcome);

        var result = await CategoryBrowsingTools.ListProductsInCategory(_mockService.Object, _logger, 1);

        result.Should().Contain("Prod in Cat");
    }
}
