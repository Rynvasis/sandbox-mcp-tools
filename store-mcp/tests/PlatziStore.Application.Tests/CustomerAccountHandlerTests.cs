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
using Xunit;

namespace PlatziStore.Application.Tests;

public class CustomerAccountHandlerTests
{
    private readonly Mock<IStoreGateway> _mockGateway;
    private readonly CustomerAccountHandler _handler;

    public CustomerAccountHandlerTests()
    {
        _mockGateway = new Mock<IStoreGateway>();
        _handler = new CustomerAccountHandler(_mockGateway.Object);
    }

    [Fact]
    public async Task ListCustomersAsync_ShouldReturnMappedProfiles_WhenSuccessful()
    {
        var users = new[] { TestDataFactory.CreateStoreCustomer(id: 1) };
        _mockGateway.Setup(x => x.GetAllUsersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        var result = await _handler.ListCustomersAsync();

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task ListCustomersAsync_ShouldReturnFailure_WhenGatewayThrows()
    {
        _mockGateway.Setup(x => x.GetAllUsersAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ExternalServiceException("Error", "Details"));

        var result = await _handler.ListCustomersAsync();

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Error");
    }

    [Fact]
    public async Task GetCustomerByIdAsync_ShouldReturnMappedProfile_WhenSuccessful()
    {
        var user = TestDataFactory.CreateStoreCustomer(id: 10);
        _mockGateway.Setup(x => x.GetUserByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _handler.GetCustomerByIdAsync(10);

        result.IsSuccess.Should().BeTrue();
        result.Data!.Id.Should().Be(10);
    }

    [Fact]
    public async Task GetCustomerByIdAsync_ShouldReturnFailure_WhenNotFound()
    {
        _mockGateway.Setup(x => x.GetUserByIdAsync(99, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException("Customer", 99));

        var result = await _handler.GetCustomerByIdAsync(99);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().ContainEquivalentOf("not found");
    }

    [Fact]
    public async Task RegisterCustomerAsync_ShouldReturnMappedProfile_WhenSuccessful()
    {
        var request = new CustomerRegistration { Name = "John", Email = "john@example.com", Password = "pass", Avatar = "img" };
        var createdUser = TestDataFactory.CreateStoreCustomer(id: 1, name: "John");
        
        _mockGateway.Setup(x => x.CreateUserAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdUser);

        var result = await _handler.RegisterCustomerAsync(request);

        result.IsSuccess.Should().BeTrue();
        result.Data!.DisplayName.Should().Be("John");
    }

    [Fact]
    public async Task RegisterCustomerAsync_ShouldReturnFailure_WhenValidationFails()
    {
        var request = new CustomerRegistration { Name = "", Email = "john@example.com", Password = "pass" };

        var result = await _handler.RegisterCustomerAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Name is required");
        _mockGateway.Verify(x => x.CreateUserAsync(It.IsAny<CustomerRegistration>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateCustomerProfileAsync_ShouldReturnMappedProfile_WhenSuccessful()
    {
        var request = new CustomerRegistration { Name = "John Updated", Email = "john@example.com", Password = "pass", Avatar = "img" };
        var updatedUser = TestDataFactory.CreateStoreCustomer(id: 1, name: "John Updated");
        
        _mockGateway.Setup(x => x.UpdateUserAsync(1, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedUser);

        var result = await _handler.UpdateCustomerProfileAsync(1, request);

        result.IsSuccess.Should().BeTrue();
        result.Data!.DisplayName.Should().Be("John Updated");
    }

    [Fact]
    public async Task UpdateCustomerProfileAsync_ShouldReturnFailure_WhenValidationFails()
    {
        var request = new CustomerRegistration { Name = "John", Email = "", Password = "pass" };

        var result = await _handler.UpdateCustomerProfileAsync(1, request);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Email is required");
    }

    [Fact]
    public async Task CheckEmailAvailabilityAsync_ShouldReturnResult_WhenSuccessful()
    {
        _mockGateway.Setup(x => x.CheckEmailAvailabilityAsync("john@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.CheckEmailAvailabilityAsync("john@example.com");

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();
    }

    [Fact]
    public async Task CheckEmailAvailabilityAsync_ShouldReturnFailure_WhenEmailEmpty()
    {
        var result = await _handler.CheckEmailAvailabilityAsync("");

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Email is required");
    }
}
