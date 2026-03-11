using System.ComponentModel;
using ModelContextProtocol.Server;
using PlatziStore.Application.Contracts;
using PlatziStore.Application.DataTransfer;
using PlatziStore.Host.Formatting;
using PlatziStore.Shared.Models;
using System.Threading.Tasks;
using System;
using PlatziStore.Infrastructure.Observability;

namespace PlatziStore.Host.Tools;

[McpServerToolType]
public static class SandboxBridgeTools
{
    [McpServerTool(Name = "analyze_store_data")]
    [Description("Fetches store data (products, categories, or users) and exports it as structured JSON for analysis. Pass the output to `write_file` on `sandbox-file`, then run analysis via `execute_python` on `sandbox-python`.")]
    public static Task<string> AnalyzeStoreData(
        ICatalogQueryService catalogService,
        ICategoryQueryService categoryService,
        ICustomerAccountService customerService,
        StructuredEventLogger logger,
        [Description("Data domain to export: products, categories, or users. Default: products.")] string dataScope = "products",
        [Description("Number of items to skip (products scope only).")] int? offset = null,
        [Description("Max items to return (products scope only, default 10).")] int? limit = null)
    {
        return logger.ExecuteToolAsync("analyze_store_data", new { dataScope, offset, limit }, async () =>
        {
            try
            {
                switch (dataScope.ToLowerInvariant())
                {
                    case "products":
                        return await HandleAnalyzeProducts(catalogService, offset, limit);
                    case "categories":
                        return await HandleAnalyzeCategories(categoryService);
                    case "users":
                        return await HandleAnalyzeUsers(customerService);
                    default:
                        return $"Error: Invalid data scope '{dataScope}'. Valid options: products, categories, users.";
                }
            }
            catch (Exception ex)
            {
                return $"Error: An unexpected error occurred: {ex.Message}";
            }
        });
    }

    [McpServerTool(Name = "process_store_export")]
    [Description("Exports store data as newline-delimited JSON (NDJSON) for command-line processing. Pass the output to `write_file` on `sandbox-file`, then process via `execute_command` on `sandbox-bash` using tools like `jq`, `awk`, or `grep`.")]
    public static Task<string> ProcessStoreExport(
        ICatalogQueryService catalogService,
        ICategoryQueryService categoryService,
        ICustomerAccountService customerService,
        StructuredEventLogger logger,
        [Description("Data domain to export: products, categories, or users. Default: products.")] string dataScope = "products",
        [Description("Number of items to skip (products scope only).")] int? offset = null,
        [Description("Max items to return (products scope only, default 10).")] int? limit = null)
    {
        return logger.ExecuteToolAsync("process_store_export", new { dataScope, offset, limit }, async () =>
        {
            try
            {
                switch (dataScope.ToLowerInvariant())
                {
                    case "products":
                        return await HandleProcessProducts(catalogService, offset, limit);
                    case "categories":
                        return await HandleProcessCategories(categoryService);
                    case "users":
                        return await HandleProcessUsers(customerService);
                    default:
                        return $"Error: Invalid data scope '{dataScope}'. Valid options: products, categories, users.";
                }
            }
            catch (Exception ex)
            {
                return $"Error: An unexpected error occurred: {ex.Message}";
            }
        });
    }

    private static async Task<string> HandleAnalyzeProducts(ICatalogQueryService catalogService, int? offset, int? limit)
    {
        var pagination = new PaginationEnvelope { Offset = offset, Limit = limit };
        var outcome = await catalogService.ListProductsAsync(pagination);
        if (!outcome.IsSuccess) return $"Error: {outcome.ErrorMessage}";
        
        var pagedData = outcome.Data;
        if (pagedData == null || pagedData.Items.Count == 0) return "No products found.";
        
        return ResponseFormatter.FormatStoreExport(
            "products", 
            pagedData.Items.Count, 
            new PaginationEnvelope { Offset = pagedData.Offset, Limit = pagedData.Limit }, 
            pagedData.Items);
    }

    private static async Task<string> HandleAnalyzeCategories(ICategoryQueryService categoryService)
    {
        var outcome = await categoryService.ListCategoriesAsync();
        if (!outcome.IsSuccess) return $"Error: {outcome.ErrorMessage}";
        
        var data = outcome.Data;
        if (data == null || data.Count == 0) return "No categories found.";
        
        return ResponseFormatter.FormatStoreExport("categories", data.Count, null, data);
    }

    private static async Task<string> HandleAnalyzeUsers(ICustomerAccountService customerService)
    {
        var outcome = await customerService.ListCustomersAsync();
        if (!outcome.IsSuccess) return $"Error: {outcome.ErrorMessage}";
        
        var data = outcome.Data;
        if (data == null || data.Count == 0) return "No users found.";
        
        return ResponseFormatter.FormatStoreExport("users", data.Count, null, data);
    }

    private static async Task<string> HandleProcessProducts(ICatalogQueryService catalogService, int? offset, int? limit)
    {
        var pagination = new PaginationEnvelope { Offset = offset, Limit = limit };
        var outcome = await catalogService.ListProductsAsync(pagination);
        if (!outcome.IsSuccess) return $"Error: {outcome.ErrorMessage}";
        
        var pagedData = outcome.Data;
        if (pagedData == null || pagedData.Items.Count == 0) return "No products found.";
        
        return ResponseFormatter.FormatNdjsonExport(pagedData.Items);
    }

    private static async Task<string> HandleProcessCategories(ICategoryQueryService categoryService)
    {
        var outcome = await categoryService.ListCategoriesAsync();
        if (!outcome.IsSuccess) return $"Error: {outcome.ErrorMessage}";
        
        var data = outcome.Data;
        if (data == null || data.Count == 0) return "No categories found.";
        
        return ResponseFormatter.FormatNdjsonExport(data);
    }

    private static async Task<string> HandleProcessUsers(ICustomerAccountService customerService)
    {
        var outcome = await customerService.ListCustomersAsync();
        if (!outcome.IsSuccess) return $"Error: {outcome.ErrorMessage}";
        
        var data = outcome.Data;
        if (data == null || data.Count == 0) return "No users found.";
        
        return ResponseFormatter.FormatNdjsonExport(data);
    }
}
