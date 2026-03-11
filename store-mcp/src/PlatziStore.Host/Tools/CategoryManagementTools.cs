using System.ComponentModel;
using ModelContextProtocol.Server;
using PlatziStore.Application.Contracts;
using PlatziStore.Application.DataTransfer;
using PlatziStore.Host.Formatting;
using PlatziStore.Infrastructure.Observability;

namespace PlatziStore.Host.Tools;

[McpServerToolType]
public static class CategoryManagementTools
{
    [McpServerTool(Name = "create_store_category")]
    [Description("Creates a new product category in the store.")]
    public static Task<string> CreateStoreCategory(
        ICategoryCommandService service,
        StructuredEventLogger logger,
        [Description("The name of the new category.")] string name,
        [Description("A valid image URL representing the category (e.g., https://placeimg.com/640/480/any).")] string image)
    {
        return logger.ExecuteToolAsync("create_store_category", new { name, image }, async () =>
        {
            var payload = new CategoryPayload
        {
            Name = name,
            Image = image
        };
        var outcome = await service.CreateCategoryAsync(payload);
        return ResponseFormatter.FormatOutcome(outcome, ResponseFormatter.FormatCategory);
        });
    }

    [McpServerTool(Name = "update_store_category")]
    [Description("Updates an existing product category. Provide the category ID and any fields you want to update.")]
    public static Task<string> UpdateStoreCategory(
        ICategoryCommandService service,
        StructuredEventLogger logger,
        [Description("The integer ID of the category to update.")] int categoryId,
        [Description("New name for the category.")] string? name = null,
        [Description("New image URL for the category.")] string? image = null)
    {
        return logger.ExecuteToolAsync("update_store_category", new { categoryId, name, image }, async () =>
        {
            var payload = new CategoryPayload
        {
            Name = name ?? string.Empty,
            Image = image ?? string.Empty
        };
        var outcome = await service.UpdateCategoryAsync(categoryId, payload);
        return ResponseFormatter.FormatOutcome(outcome, ResponseFormatter.FormatCategory);
        });
    }

    [McpServerTool(Name = "remove_store_category")]
    [Description("Removes a category from the store by its ID.")]
    public static Task<string> RemoveStoreCategory(
        ICategoryCommandService service,
        StructuredEventLogger logger,
        [Description("The integer ID of the category to delete.")] int categoryId)
    {
        return logger.ExecuteToolAsync("remove_store_category", new { categoryId }, async () =>
        {
            var outcome = await service.DeleteCategoryAsync(categoryId);
            return outcome.IsSuccess 
                ? $"Successfully removed category #{categoryId}." 
                : $"Error removing category: {outcome.ErrorMessage}";
        });
    }
}
