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

public class CatalogManagementToolsTests
{
    private readonly Mock<ICatalogCommandService> _mockService;
    private readonly StructuredEventLogger _logger;

    public CatalogManagementToolsTests()
    {
        _mockService = new Mock<ICatalogCommandService>();
        
        var metrics = new ToolInvocationMetrics();
        var options = Options.Create(new TelemetryOptions { MetricsEnabled = false });
        var loggerMock = new Mock<ILogger<StructuredEventLogger>>();
        _logger = new StructuredEventLogger(loggerMock.Object, options, metrics);
    }

    [Fact]
    public async Task CreateStoreProduct_ReturnsFormattedDetail_WhenSuccessful()
    {
        var detail = new CatalogItemDetail { Id = 1, Title = "New Prod" };
        var outcome = OperationOutcome<CatalogItemDetail>.Success(detail);
            
        _mockService.Setup(x => x.CreateProductAsync(It.IsAny<CatalogItemPayload>(), default))
            .ReturnsAsync(outcome);

        var result = await CatalogManagementTools.CreateStoreProduct(_mockService.Object, _logger, 
            "New Prod", 10m, "Desc", 1, new[] { "img.jpg" });

        result.Should().Contain("New Prod");
    }

    [Fact]
    public async Task UpdateStoreProduct_ReturnsFormattedDetail_WhenSuccessful()
    {
        var detail = new CatalogItemDetail { Id = 1, Title = "Updated Prod" };
        var outcome = OperationOutcome<CatalogItemDetail>.Success(detail);
            
        _mockService.Setup(x => x.UpdateProductAsync(1, It.IsAny<CatalogItemPayload>(), default))
            .ReturnsAsync(outcome);

        var result = await CatalogManagementTools.UpdateStoreProduct(_mockService.Object, _logger, 
            1, "Updated Prod", 15m, "New Desc", 2, new[] { "img2.jpg" });

        result.Should().Contain("Updated Prod");
    }

    [Fact]
    public async Task RemoveStoreProduct_ReturnsFormattedSuccess_WhenSuccessful()
    {
        var outcome = OperationOutcome<bool>.Success(true);
            
        _mockService.Setup(x => x.DeleteProductAsync(1, default))
            .ReturnsAsync(outcome);

        var result = await CatalogManagementTools.RemoveStoreProduct(_mockService.Object, _logger, 1);

        result.Should().Contain("Successfully removed");
    }
}
