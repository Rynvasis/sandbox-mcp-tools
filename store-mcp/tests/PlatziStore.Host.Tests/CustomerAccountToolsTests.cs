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

public class CustomerAccountToolsTests
{
    private readonly Mock<ICustomerAccountService> _mockService;
    private readonly StructuredEventLogger _logger;

    public CustomerAccountToolsTests()
    {
        _mockService = new Mock<ICustomerAccountService>();
        
        var metrics = new ToolInvocationMetrics();
        var options = Options.Create(new TelemetryOptions { MetricsEnabled = false });
        var loggerMock = new Mock<ILogger<StructuredEventLogger>>();
        _logger = new StructuredEventLogger(loggerMock.Object, options, metrics);
    }

    [Fact]
    public async Task ListStoreCustomers_ReturnsFormattedList_WhenSuccessful()
    {
        var items = new List<CustomerProfile> { new() { Id = 1, DisplayName = "User A" } };
        var outcome = OperationOutcome<IReadOnlyList<CustomerProfile>>.Success(items);
            
        _mockService.Setup(x => x.ListCustomersAsync(default))
            .ReturnsAsync(outcome);

        var result = await CustomerAccountTools.ListStoreCustomers(_mockService.Object, _logger);

        result.Should().Contain("User A");
    }

    [Fact]
    public async Task GetCustomerById_ReturnsFormattedDetail_WhenSuccessful()
    {
        var item = new CustomerProfile { Id = 1, DisplayName = "User A" };
        var outcome = OperationOutcome<CustomerProfile>.Success(item);
            
        _mockService.Setup(x => x.GetCustomerByIdAsync(1, default))
            .ReturnsAsync(outcome);

        var result = await CustomerAccountTools.GetCustomerById(_mockService.Object, _logger, 1);

        result.Should().Contain("User A");
    }

    [Fact]
    public async Task RegisterCustomer_ReturnsFormattedDetail_WhenSuccessful()
    {
        var item = new CustomerProfile { Id = 1, DisplayName = "User A" };
        var outcome = OperationOutcome<CustomerProfile>.Success(item);
            
        _mockService.Setup(x => x.RegisterCustomerAsync(It.IsAny<CustomerRegistration>(), default))
            .ReturnsAsync(outcome);

        var result = await CustomerAccountTools.RegisterCustomer(_mockService.Object, _logger, "User A", "test@test.com", "pass", "img.jpg");

        result.Should().Contain("User A");
    }

    [Fact]
    public async Task UpdateCustomerProfile_ReturnsFormattedDetail_WhenSuccessful()
    {
        var item = new CustomerProfile { Id = 1, DisplayName = "User B" };
        var outcome = OperationOutcome<CustomerProfile>.Success(item);
            
        _mockService.Setup(x => x.UpdateCustomerProfileAsync(1, It.IsAny<CustomerRegistration>(), default))
            .ReturnsAsync(outcome);

        var result = await CustomerAccountTools.UpdateCustomerProfile(_mockService.Object, _logger, 1, "User B", "test@test.com", "pass", "img.jpg");

        result.Should().Contain("User B");
    }

    [Fact]
    public async Task CheckEmailAvailability_ReturnsFormattedResult_WhenSuccessful()
    {
        var outcome = OperationOutcome<bool>.Success(true);
            
        _mockService.Setup(x => x.CheckEmailAvailabilityAsync("test@ex.com", default))
            .ReturnsAsync(outcome);

        var result = await CustomerAccountTools.CheckEmailAvailability(_mockService.Object, _logger, "test@ex.com");

        result.Should().Contain("is available");
    }
}
