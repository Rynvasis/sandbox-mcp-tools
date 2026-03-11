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
using PlatziStore.Shared.Exceptions;
using Xunit;

namespace PlatziStore.Application.Tests;

public class IdentityAccessHandlerTests
{
    private readonly Mock<IStoreGateway> _mockGateway;
    private readonly IdentityAccessHandler _handler;

    public IdentityAccessHandlerTests()
    {
        _mockGateway = new Mock<IStoreGateway>();
        _handler = new IdentityAccessHandler(_mockGateway.Object);
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldReturnTokens_WhenSuccessful()
    {
        var credentials = new AuthCredentials { Email = "john@example.com", Password = "password" };
        var tokens = new AuthTokenPair { AccessToken = "access", RefreshToken = "refresh" };
        
        _mockGateway.Setup(x => x.LoginAsync(credentials.Email, credentials.Password, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tokens);

        var result = await _handler.AuthenticateAsync(credentials);

        result.IsSuccess.Should().BeTrue();
        result.Data!.AccessToken.Should().Be("access");
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldReturnFailure_WhenValidationFails()
    {
        var credentials = new AuthCredentials { Email = "", Password = "password" };

        var result = await _handler.AuthenticateAsync(credentials);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Email is required");
    }

    [Fact]
    public async Task AuthenticateAsync_ShouldReturnFailure_WhenGatewayThrowsAuthorizationDenied()
    {
        var credentials = new AuthCredentials { Email = "john@example.com", Password = "wrongpassword" };
        // Assuming AuthorizationDeniedException is in PlatziStore.Shared.Exceptions or Domain
        // If not found compiler will complain. Let's create a mocked exception type.
        // The service does: catch (Exception ex) when (ex.GetType().Name == "AuthorizationDeniedException")
        // So we can just throw a generic Exception named AuthorizationDeniedException if it exists, 
        // or a derived class to simulate. Let's just create a dynamic Mock or a concrete exception that matches the name.
        // For simplicity, we can throw an exception from PlatziStore.Shared.Exceptions.AuthorizationDeniedException if it exists.
        
        // Use a dummy class in the test method just to simulate
        _mockGateway.Setup(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AuthorizationDeniedException("Denied"));

        var result = await _handler.AuthenticateAsync(credentials);

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Invalid email or password");
    }

    [Fact]
    public async Task GetAuthenticatedProfileAsync_ShouldReturnProfile_WhenSuccessful()
    {
        var user = TestDataFactory.CreateStoreCustomer(id: 1, name: "John");
        _mockGateway.Setup(x => x.GetProfileAsync("access", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _handler.GetAuthenticatedProfileAsync("access");

        result.IsSuccess.Should().BeTrue();
        result.Data!.DisplayName.Should().Be("John");
    }

    [Fact]
    public async Task GetAuthenticatedProfileAsync_ShouldReturnFailure_WhenTokenEmpty()
    {
        var result = await _handler.GetAuthenticatedProfileAsync("");

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Access token is required");
    }

    [Fact]
    public async Task RefreshSessionAsync_ShouldReturnTokens_WhenSuccessful()
    {
        var tokens = new AuthTokenPair { AccessToken = "new_access", RefreshToken = "new_refresh" };
        _mockGateway.Setup(x => x.RefreshTokenAsync("refresh", It.IsAny<CancellationToken>()))
            .ReturnsAsync(tokens);

        var result = await _handler.RefreshSessionAsync("refresh");

        result.IsSuccess.Should().BeTrue();
        result.Data!.AccessToken.Should().Be("new_access");
    }

    [Fact]
    public async Task RefreshSessionAsync_ShouldReturnFailure_WhenTokenEmpty()
    {
        var result = await _handler.RefreshSessionAsync("");

        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Refresh token is required");
    }
}

// Simulate AuthorizationDeniedException since it might not be globally imported or defined easily
public class AuthorizationDeniedException : Exception
{
    public AuthorizationDeniedException(string message) : base(message) { }
}
