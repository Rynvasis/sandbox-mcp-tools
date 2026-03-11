using System.ComponentModel;
using ModelContextProtocol.Server;
using PlatziStore.Application.Contracts;
using PlatziStore.Application.DataTransfer;
using PlatziStore.Host.Formatting;
using PlatziStore.Infrastructure.Observability;

namespace PlatziStore.Host.Tools;

[McpServerToolType]
public static class CatalogManagementTools
{
    [McpServerTool(Name = "create_store_product")]
    [Description("Creates a new product in the store with the given details.")]
    public static Task<string> CreateStoreProduct(
        ICatalogCommandService service,
        StructuredEventLogger logger,
        [Description("The title of the new product.")] string title,
        [Description("The numeric price of the product (will be treated as integer internally).")] decimal price,
        [Description("A detailed description of the product.")] string description,
        [Description("The integer ID of the category this product belongs to.")] int categoryId,
        [Description("An array of image URLs showing the product. Must contain at least one valid image URL (e.g., https://placeimg.com/640/480/any).")] string[] images)
    {
        return logger.ExecuteToolAsync("create_store_product", new { title, price, description, categoryId, images }, async () =>
        {
            var payload = new CatalogItemPayload
        {
            Title = title,
            Price = price,
            Description = description,
            CategoryId = categoryId,
            Images = images
        };
        var outcome = await service.CreateProductAsync(payload);
        return ResponseFormatter.FormatOutcome(outcome, ResponseFormatter.FormatProductDetail);
        });
    }

    [McpServerTool(Name = "update_store_product")]
    [Description("Updates an existing product in the store. Provide the product ID and any fields you want to update. Missing fields will remain unchanged.")]
    public static Task<string> UpdateStoreProduct(
        ICatalogCommandService service,
        StructuredEventLogger logger,
        [Description("The integer ID of the product to update.")] int productId,
        [Description("New title for the product.")] string? title = null,
        [Description("New price for the product.")] decimal? price = null,
        [Description("New description for the product.")] string? description = null,
        [Description("New category ID for the product.")] int? categoryId = null,
        [Description("New array of image URLs for the product.")] string[]? images = null)
    {
        return logger.ExecuteToolAsync("update_store_product", new { productId, title, price, description, categoryId, images }, async () =>
        {
            var payload = new CatalogItemPayload
        {
            Title = title ?? string.Empty,
            Price = price ?? 0,
            Description = description ?? string.Empty,
            CategoryId = categoryId ?? 0,
            Images = images ?? []
        };
        // The Application layer requires merging logic or assumes payload contains all fields. 
        // According to the previous implementation, the handler retrieves the product and merges only provided (non-default) values.
        var outcome = await service.UpdateProductAsync(productId, payload);
        return ResponseFormatter.FormatOutcome(outcome, ResponseFormatter.FormatProductDetail);
        });
    }

    [McpServerTool(Name = "remove_store_product")]
    [Description("Removes a product from the store by its ID.")]
    public static Task<string> RemoveStoreProduct(
        ICatalogCommandService service,
        StructuredEventLogger logger,
        [Description("The integer ID of the product to delete.")] int productId)
    {
        return logger.ExecuteToolAsync("remove_store_product", new { productId }, async () =>
        {
            var outcome = await service.DeleteProductAsync(productId);
            return outcome.IsSuccess 
                ? $"Successfully removed product #{productId}." 
                : $"Error removing product: {outcome.ErrorMessage}";
        });
    }
}
