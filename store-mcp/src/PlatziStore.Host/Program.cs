using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PlatziStore.Application.Contracts;
using PlatziStore.Application.DependencyInjection;
using PlatziStore.Host.Adapters;
using PlatziStore.Infrastructure.DependencyInjection;

namespace PlatziStore.Host;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder(args);

        // MCP stdio transport requires that ONLY JSON-RPC messages appear on stdout.
        // Clear all default logging providers (which write to stdout/console) to prevent
        // non-JSON text from polluting the MCP transport channel.
        builder.Logging.ClearProviders();
        builder.Logging.AddDebug(); // Debug provider writes to stderr, safe for MCP

        // Host.CreateApplicationBuilder automatically loads appsettings.json from the content root.

        // Register Infrastructure Layer (includes IPlatziStoreGateway and HttpClient)
        builder.Services.AddPlatziStoreInfrastructure(builder.Configuration);

        // Register Application Layer (includes all command/query handlers)
        builder.Services.AddPlatziStoreApplication();

        // Register Adapter to bridge Application IStoreGateway -> Infrastructure IPlatziStoreGateway
        builder.Services.AddScoped<IStoreGateway, StoreGatewayAdapter>();

        // Configure MCP Server on stdio transport
        builder.Services.AddMcpServer(options =>
        {
            options.ServerInfo = new() { Name = "PlatziStoreServer", Version = "1.0.0" };
        }).WithStdioServerTransport()
          .WithToolsFromAssembly();

        var host = builder.Build();
        
        await host.RunAsync();
    }
}

