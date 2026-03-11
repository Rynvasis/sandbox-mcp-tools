using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using PlatziStore.Infrastructure.Configuration;
using PlatziStore.Infrastructure.Observability;
using Xunit;

namespace PlatziStore.Infrastructure.Tests;

public class ObservabilityTests
{
    [Fact]
    public void StructuredEventLogger_RedactsSensitiveData()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<StructuredEventLogger>>();
        var options = Options.Create(new TelemetryOptions { MetricsEnabled = true, MetricsSummaryInterval = 10 });
        var metrics = new ToolInvocationMetrics();
        var logger = new StructuredEventLogger(mockLogger.Object, options, metrics);

        var parameters = new
        {
            Username = "testuser",
            Password = "secretpassword",
            Token = "secrettoken"
        };

        // Act
        logger.LogToolStarted("test_tool", parameters);

        // Assert
        // Verify Log was called with a message containing [REDACTED] and not containing the secret values
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString()!.Contains("[REDACTED]") && 
                    !v.ToString()!.Contains("secretpassword") && 
                    !v.ToString()!.Contains("secrettoken") &&
                    v.ToString()!.Contains("testuser")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ToolInvocationMetrics_IsThreadSafe()
    {
        // Arrange
        var metrics = new ToolInvocationMetrics();
        var parallelTasks = 100;
        var callsPerTask = 100;

        // Act
        var tasks = Enumerable.Range(0, parallelTasks).Select(_ => Task.Run(() =>
        {
            for (int i = 0; i < callsPerTask; i++)
            {
                metrics.RecordApiCall(200, 10);
                metrics.RecordApiCall(404, 5);
            }
        }));

        await Task.WhenAll(tasks);

        // Assert
        var snapshot = metrics.GetSnapshot();
        Assert.Equal(parallelTasks * callsPerTask * 2, snapshot.TotalCalls);
        
        // 10ms + 5ms = 15ms per inner loop
        Assert.Equal(parallelTasks * callsPerTask * 15, snapshot.TotalDurationMs);
        Assert.Equal(10, snapshot.MaxDurationMs); // Max of 10 and 5
        
        Assert.Equal(parallelTasks * callsPerTask, snapshot.StatusCodeCounts[200]);
        Assert.Equal(parallelTasks * callsPerTask, snapshot.StatusCodeCounts[404]);
    }

    [Fact]
    public void StructuredEventLogger_LogsSummary_AtInterval()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<StructuredEventLogger>>();
        var options = Options.Create(new TelemetryOptions { MetricsEnabled = true, MetricsSummaryInterval = 5 });
        var metrics = new ToolInvocationMetrics();
        var logger = new StructuredEventLogger(mockLogger.Object, options, metrics);

        // Act & Assert
        for (int i = 1; i <= 5; i++)
        {
            metrics.RecordApiCall(200, 100);
            logger.LogApiCallCompleted("/test", 200, 100);
            
            if (i < 5)
            {
                // Shouldn't have logged summary yet
                mockLogger.Verify(
                    x => x.Log(
                        LogLevel.Information,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("API Metrics Summary")),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Never);
            }
            else
            {
                // After 5th call, should log summary
                mockLogger.Verify(
                    x => x.Log(
                        LogLevel.Information,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("API Metrics Summary")),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once);
            }
        }
    }
}
