using System.Collections.Concurrent;

namespace PlatziStore.Infrastructure.Observability;

public record ToolInvocationMetricsSnapshot(
    long TotalCalls,
    long TotalDurationMs,
    long MaxDurationMs,
    IReadOnlyDictionary<int, long> StatusCodeCounts);

public class ToolInvocationMetrics
{
    private long _totalCalls;
    private long _totalDurationMs;
    private long _maxDurationMs;
    private ConcurrentDictionary<int, long> _statusCodeCounts = new();

    public void RecordApiCall(int statusCode, long durationMs)
    {
        Interlocked.Increment(ref _totalCalls);
        Interlocked.Add(ref _totalDurationMs, durationMs);

        long currentMax;
        long newMax;
        do
        {
            currentMax = Interlocked.Read(ref _maxDurationMs);
            if (durationMs <= currentMax) break;
            newMax = durationMs;
        } while (Interlocked.CompareExchange(ref _maxDurationMs, newMax, currentMax) != currentMax);

        _statusCodeCounts.AddOrUpdate(statusCode, 1, (_, count) => count + 1);
    }

    public ToolInvocationMetricsSnapshot GetSnapshot()
    {
        return new ToolInvocationMetricsSnapshot(
            Interlocked.Read(ref _totalCalls),
            Interlocked.Read(ref _totalDurationMs),
            Interlocked.Read(ref _maxDurationMs),
            new Dictionary<int, long>(_statusCodeCounts)); // Create snapshot
    }

    public void Reset()
    {
        Interlocked.Exchange(ref _totalCalls, 0);
        Interlocked.Exchange(ref _totalDurationMs, 0);
        Interlocked.Exchange(ref _maxDurationMs, 0);
        _statusCodeCounts.Clear();
    }
}
