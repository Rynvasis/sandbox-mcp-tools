namespace PlatziStore.Infrastructure.Configuration;

public class TelemetryOptions
{
    public string LogLevel { get; set; } = "Information";
    public bool MetricsEnabled { get; set; } = true;
    public int MetricsSummaryInterval { get; set; } = 50;
}
