using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using PlatziStore.Infrastructure.ApiClients;
using PlatziStore.Infrastructure.Configuration;

namespace PlatziStore.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddPlatziStoreInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PlatziStoreOptions>(configuration.GetSection("PlatziStore"));

        services.AddSingleton<PlatziStoreResponseParser>();

        services.AddHttpClient<IPlatziStoreGateway, PlatziStoreGateway>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<PlatziStoreOptions>>().Value;
            
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        })
        .AddPolicyHandler((serviceProvider, request) => 
        {
            var options = serviceProvider.GetRequiredService<IOptions<PlatziStoreOptions>>().Value;
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(options.RetryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        });

        return services;
    }
}
