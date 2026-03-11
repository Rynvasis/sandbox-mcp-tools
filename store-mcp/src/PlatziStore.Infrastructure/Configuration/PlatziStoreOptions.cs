namespace PlatziStore.Infrastructure.Configuration;

public class PlatziStoreOptions
{
    public string BaseUrl { get; set; } = "https://api.escuelajs.co";
    public int TimeoutSeconds { get; set; } = 30;
    public int RetryCount { get; set; } = 3;
}
