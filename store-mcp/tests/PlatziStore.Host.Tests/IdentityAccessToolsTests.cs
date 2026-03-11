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

public class IdentityAccessToolsTests
{
    private readonly Mock<IIdentityAccessService> _mockService;
    private readonly StructuredEventLogger _logger;

    public IdentityAccessToolsTests()
    {
        _mockService = new Mock<IIdentityAccessService>();
        
        var metrics = new ToolInvocationMetrics();
        var options = Options.Create(new TelemetryOptions { MetricsEnabled = false });
        var loggerMock = new Mock<ILogger<StructuredEventLogger>>();
        _logger = new StructuredEventLogger(loggerMock.Object, options, metrics);
    }

    [Fact]
    public async Task AuthenticateCustomer_ReturnsTokens_WhenSuccessful()
    {
        var tokens = new AuthTokenPair { AccessToken = "acc", RefreshToken = "ref" };
        var outcome = OperationOutcome<AuthTokenPair>.Success(tokens);
            
        _mockService.Setup(x => x.AuthenticateAsync(It.IsAny<AuthCredentials>(), default))
            .ReturnsAsync(outcome);

        var result = await IdentityAccessTools.AuthenticateCustomer(_mockService.Object, _logger, "e@e.com", "pass");

        result.Should().Contain("acc");
        result.Should().Contain("ref");
    }

    [Fact]
    public async Task GetAuthenticatedProfile_ReturnsProfile_WhenSuccessful()
    {
        var profile = new CustomerProfile { Id = 1, DisplayName = "Auth User" };
        var outcome = OperationOutcome<CustomerProfile>.Success(profile);
            
        _mockService.Setup(x => x.GetAuthenticatedProfileAsync("acc", default))
            .ReturnsAsync(outcome);

        var result = await IdentityAccessTools.GetAuthenticatedProfile(_mockService.Object, _logger, "acc");

        result.Should().Contain("Auth User");
    }

    [Fact]
    public async Task RefreshAccessToken_ReturnsTokens_WhenSuccessful()
    {
        var tokens = new AuthTokenPair { AccessToken = "new_acc", RefreshToken = "new_ref" };
        var outcome = OperationOutcome<AuthTokenPair>.Success(tokens);
            
        _mockService.Setup(x => x.RefreshSessionAsync("ref", default))
            .ReturnsAsync(outcome);

        var result = await IdentityAccessTools.RefreshAccessToken(_mockService.Object, _logger, "ref");

        result.Should().Contain("new_acc");
        result.Should().Contain("new_ref");
    }
}
