using System.ComponentModel;
using ModelContextProtocol.Server;
using PlatziStore.Application.Contracts;
using PlatziStore.Application.DataTransfer;
using PlatziStore.Host.Formatting;
using PlatziStore.Shared.Models;
using PlatziStore.Infrastructure.Observability;

namespace PlatziStore.Host.Tools;

[McpServerToolType]
public static class CategoryBrowsingTools
{
    [McpServerTool(Name = "list_store_categories")]
    [Description("Lists all product categories available in the store. Use this to discover how products are grouped.")]
    public static Task<string> ListStoreCategories(
        ICategoryQueryService service,
        StructuredEventLogger logger)
    {
        return logger.ExecuteToolAsync("list_store_categories", null, async () =>
        {
            var outcome = await service.ListCategoriesAsync();
            return ResponseFormatter.FormatOutcome(outcome, ResponseFormatter.FormatCategoryList);
        });
    }

    [McpServerTool(Name = "get_category_by_id")]
    [Description("Gets detailed information about a specific category by its integer ID.")]
    public static Task<string> GetCategoryById(
        ICategoryQueryService service,
        StructuredEventLogger logger,
        [Description("The integer ID of the category.")] int categoryId)
    {
        return logger.ExecuteToolAsync("get_category_by_id", new { categoryId }, async () =>
        {
            var outcome = await service.GetCategoryByIdAsync(categoryId);
            return ResponseFormatter.FormatOutcome(outcome, ResponseFormatter.FormatCategory);
        });
    }

    [McpServerTool(Name = "get_category_by_slug")]
    [Description("Gets detailed information about a category by its URL-friendly slug.")]
    public static Task<string> GetCategoryBySlug(
        ICategoryQueryService service,
        StructuredEventLogger logger,
        [Description("The string slug of the category.")] string slug)
    {
        return logger.ExecuteToolAsync("get_category_by_slug", new { slug }, async () =>
        {
            var outcome = await service.GetCategoryBySlugAsync(slug);
            return ResponseFormatter.FormatOutcome(outcome, ResponseFormatter.FormatCategory);
        });
    }

    [McpServerTool(Name = "list_products_in_category")]
    [Description("Lists all products belonging to a specific category with optional pagination.")]
    public static Task<string> ListProductsInCategory(
        ICategoryQueryService service,
        StructuredEventLogger logger,
        [Description("The integer ID of the category.")] int categoryId,
        [Description("Number of items to skip. Defaults to 0.")] int? offset = null,
        [Description("Maximum number of items to return. Defaults to 10.")] int? limit = null)
    {
        return logger.ExecuteToolAsync("list_products_in_category", new { categoryId, offset, limit }, async () =>
        {
            var pagination = new PaginationEnvelope { Offset = offset, Limit = limit };
            var outcome = await service.ListProductsByCategoryAsync(categoryId, pagination);
            return ResponseFormatter.FormatOutcome(outcome, ResponseFormatter.FormatProductList);
        });
    }
}
