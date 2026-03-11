using System.ComponentModel;
using ModelContextProtocol.Server;
using PlatziStore.Application.Contracts;
using PlatziStore.Application.DataTransfer;
using PlatziStore.Host.Formatting;
using PlatziStore.Shared.Models;

namespace PlatziStore.Host.Tools;

[McpServerToolType]
public static class CategoryBrowsingTools
{
    [McpServerTool(Name = "list_store_categories")]
    [Description("Lists all product categories available in the store. Use this to discover how products are grouped.")]
    public static async Task<string> ListStoreCategories(
        ICategoryQueryService service)
    {
        var outcome = await service.ListCategoriesAsync();
        return ResponseFormatter.FormatOutcome(outcome, ResponseFormatter.FormatCategoryList);
    }

    [McpServerTool(Name = "get_category_by_id")]
    [Description("Gets detailed information about a specific category by its integer ID.")]
    public static async Task<string> GetCategoryById(
        ICategoryQueryService service,
        [Description("The integer ID of the category.")] int categoryId)
    {
        var outcome = await service.GetCategoryByIdAsync(categoryId);
        return ResponseFormatter.FormatOutcome(outcome, ResponseFormatter.FormatCategory);
    }

    [McpServerTool(Name = "get_category_by_slug")]
    [Description("Gets detailed information about a category by its URL-friendly slug.")]
    public static async Task<string> GetCategoryBySlug(
        ICategoryQueryService service,
        [Description("The string slug of the category.")] string slug)
    {
        var outcome = await service.GetCategoryBySlugAsync(slug);
        return ResponseFormatter.FormatOutcome(outcome, ResponseFormatter.FormatCategory);
    }

    [McpServerTool(Name = "list_products_in_category")]
    [Description("Lists all products belonging to a specific category with optional pagination.")]
    public static async Task<string> ListProductsInCategory(
        ICategoryQueryService service,
        [Description("The integer ID of the category.")] int categoryId,
        [Description("Number of items to skip. Defaults to 0.")] int? offset = null,
        [Description("Maximum number of items to return. Defaults to 10.")] int? limit = null)
    {
        var pagination = new PaginationEnvelope { Offset = offset, Limit = limit };
        var outcome = await service.ListProductsByCategoryAsync(categoryId, pagination);
        return ResponseFormatter.FormatOutcome(outcome, ResponseFormatter.FormatProductList);
    }
}
