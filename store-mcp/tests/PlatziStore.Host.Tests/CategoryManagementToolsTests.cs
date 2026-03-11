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

public class CategoryManagementToolsTests
{
    private readonly Mock<ICategoryCommandService> _mockService;
    private readonly StructuredEventLogger _logger;

    public CategoryManagementToolsTests()
    {
        _mockService = new Mock<ICategoryCommandService>();
        
        var metrics = new ToolInvocationMetrics();
        var options = Options.Create(new TelemetryOptions { MetricsEnabled = false });
        var loggerMock = new Mock<ILogger<StructuredEventLogger>>();
        _logger = new StructuredEventLogger(loggerMock.Object, options, metrics);
    }

    [Fact]
    public async Task CreateStoreCategory_ReturnsFormattedDetail_WhenSuccessful()
    {
        var detail = new CategorySummary { Id = 1, Name = "New Cat" };
        var outcome = OperationOutcome<CategorySummary>.Success(detail);
            
        _mockService.Setup(x => x.CreateCategoryAsync(It.IsAny<CategoryPayload>(), default))
            .ReturnsAsync(outcome);

        var result = await CategoryManagementTools.CreateStoreCategory(_mockService.Object, _logger, "New Cat", "img.jpg");

        result.Should().Contain("New Cat");
    }

    [Fact]
    public async Task UpdateStoreCategory_ReturnsFormattedDetail_WhenSuccessful()
    {
        var detail = new CategorySummary { Id = 1, Name = "Updated Cat" };
        var outcome = OperationOutcome<CategorySummary>.Success(detail);
            
        _mockService.Setup(x => x.UpdateCategoryAsync(1, It.IsAny<CategoryPayload>(), default))
            .ReturnsAsync(outcome);

        var result = await CategoryManagementTools.UpdateStoreCategory(_mockService.Object, _logger, 1, "Updated Cat", "img.jpg");

        result.Should().Contain("Updated Cat");
    }

    [Fact]
    public async Task RemoveStoreCategory_ReturnsFormattedSuccess_WhenSuccessful()
    {
        var outcome = OperationOutcome<bool>.Success(true);
            
        _mockService.Setup(x => x.DeleteCategoryAsync(1, default))
            .ReturnsAsync(outcome);

        var result = await CategoryManagementTools.RemoveStoreCategory(_mockService.Object, _logger, 1);

        result.Should().Contain("Successfully removed");
    }
}
