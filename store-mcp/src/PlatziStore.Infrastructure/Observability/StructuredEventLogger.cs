using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PlatziStore.Infrastructure.Configuration;
using System.Text.Json;

namespace PlatziStore.Infrastructure.Observability;

public class StructuredEventLogger
{
    private readonly ILogger<StructuredEventLogger> _logger;
    private readonly TelemetryOptions _options;
    private readonly ToolInvocationMetrics _metrics;
    private static readonly string[] SensitiveKeys = { "password", "token", "accesstoken", "refreshtoken" };

    public StructuredEventLogger(ILogger<StructuredEventLogger> logger, IOptions<TelemetryOptions> options, ToolInvocationMetrics metrics)
    {
        _logger = logger;
        _options = options.Value;
        _metrics = metrics;
    }

    public void LogToolStarted(string toolName, object? parameters)
    {
        var safeParams = RedactSensitiveData(parameters);
        _logger.LogInformation("Tool started: {ToolName} with parameters {Parameters}", toolName, JsonSerializer.Serialize(safeParams));
    }

    public void LogToolCompleted(string toolName, long durationMs)
    {
        _logger.LogInformation("Tool completed: {ToolName} (duration: {DurationMs}ms, status: success)", toolName, durationMs);
    }

    public void LogToolFailed(string toolName, long durationMs, string errorType, string errorMessage)
    {
        _logger.LogWarning("Tool failed: {ToolName} (duration: {DurationMs}ms, errorType: {ErrorType}, errorMessage: {ErrorMessage})", 
            toolName, durationMs, errorType, errorMessage);
    }

    public void LogApiCallCompleted(string endpoint, int statusCode, long durationMs)
    {
        _logger.LogDebug("API Call completed: {Endpoint} (statusCode: {StatusCode}, duration: {DurationMs}ms)", 
            endpoint, statusCode, durationMs);
            
        if (_options.MetricsEnabled)
        {
            var snapshot = _metrics.GetSnapshot();
            // Periodic summary logging
            if (snapshot.TotalCalls > 0 && snapshot.TotalCalls % _options.MetricsSummaryInterval == 0)
            {
                LogMetricsSummary(snapshot);
            }
        }
    }

    public void LogMetricsSummary(ToolInvocationMetricsSnapshot snapshot)
    {
        var avg = snapshot.TotalCalls > 0 ? snapshot.TotalDurationMs / snapshot.TotalCalls : 0;
        var statusCodes = string.Join(", ", snapshot.StatusCodeCounts.Select(kvp => $"{kvp.Key}={kvp.Value}"));
        
        _logger.LogInformation("API Metrics Summary: calls={TotalCalls}, avg={AvgMs}ms, max={MaxMs}ms, {StatusCodes}",
            snapshot.TotalCalls, avg, snapshot.MaxDurationMs, statusCodes);
    }

    public async Task<string> ExecuteToolAsync(string toolName, object? parameters, Func<Task<string>> action)
    {
        var sw = Stopwatch.StartNew();
        LogToolStarted(toolName, parameters);
        try
        {
            var result = await action();
            LogToolCompleted(toolName, sw.ElapsedMilliseconds);
            return result;
        }
        catch (Exception ex)
        {
            LogToolFailed(toolName, sw.ElapsedMilliseconds, ex.GetType().Name, ex.Message);
            throw;
        }
    }

    private object? RedactSensitiveData(object? parameters)
    {
        if (parameters == null) return null;

        if (parameters is Dictionary<string, object> dict)
        {
            var safeDict = new Dictionary<string, object>(dict, StringComparer.OrdinalIgnoreCase);
            foreach (var key in SensitiveKeys)
            {
                if (safeDict.ContainsKey(key))
                {
                    safeDict[key] = "[REDACTED]";
                }
            }
            return safeDict;
        }
        
        // If it's a typed object, serialize to JsonElement and filter
        var jsonStr = JsonSerializer.Serialize(parameters);
        var document = JsonDocument.Parse(jsonStr);
        var redactedObj = new Dictionary<string, object?>();

        foreach (var element in document.RootElement.EnumerateObject())
        {
            if (SensitiveKeys.Contains(element.Name.ToLowerInvariant()))
            {
                redactedObj[element.Name] = "[REDACTED]";
            }
            else
            {
                redactedObj[element.Name] = element.Value.Clone();
            }
        }

        return redactedObj;
    }
}
