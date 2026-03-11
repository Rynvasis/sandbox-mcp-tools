using System.ComponentModel;
using PlatziStore.Application.Contracts;
using PlatziStore.Application.DataTransfer;
using PlatziStore.Host.Formatting;
using ModelContextProtocol.Server;
using PlatziStore.Infrastructure.Observability;

namespace PlatziStore.Host.Tools;

[McpServerToolType]
public static class CatalogBrowsingTools
{
    [McpServerTool(Name = "list_store_products")]
    [Description("Lists all products in the store with optional pagination. Returns a summarized list of products. Use this to discover available products.")]
    public static Task<string> ListStoreProducts(
        ICatalogQueryService service,
        StructuredEventLogger logger,
        [Description("Number of items to skip. Defaults to 0.")] int? offset = null,
        [Description("Maximum number of items to return. Defaults to 10.")] int? limit = null)
    {
        return logger.ExecuteToolAsync("list_store_products", new { offset, limit }, async () =>
        {
            var pagination = new PaginationEnvelope { Offset = offset, Limit = limit };
            var outcome = await service.ListProductsAsync(pagination);
            return ResponseFormatter.FormatOutcome(outcome, ResponseFormatter.FormatProductList);
        });
    }

    [McpServerTool(Name = "get_product_by_id")]
    [Description("Gets detailed information about a specific product by its integer ID. Include the ID to retrieve full details, description, and images.")]
    public static Task<string> GetProductById(
        ICatalogQueryService service,
        StructuredEventLogger logger,
        [Description("The unique integer ID of the product.")] int productId)
    {
        return logger.ExecuteToolAsync("get_product_by_id", new { productId }, async () =>
        {
            var outcome = await service.GetProductByIdAsync(productId);
            return ResponseFormatter.FormatOutcome(outcome, ResponseFormatter.FormatProductDetail);
        });
    }

    [McpServerTool(Name = "get_product_by_slug")]
    [Description("Gets detailed information about a product by its URL-friendly slug. Use this if you know the product's slug string.")]
    public static Task<string> GetProductBySlug(
        ICatalogQueryService service,
        StructuredEventLogger logger,
        [Description("The string slug of the product.")] string slug)
    {
        return logger.ExecuteToolAsync("get_product_by_slug", new { slug }, async () =>
        {
            var outcome = await service.GetProductBySlugAsync(slug);
            return ResponseFormatter.FormatOutcome(outcome, ResponseFormatter.FormatProductDetail);
        });
    }

    [McpServerTool(Name = "find_related_products")]
    [Description("Finds products that are related to a given product ID (usually same category).")]
    public static Task<string> FindRelatedProducts(
        ICatalogQueryService service,
        StructuredEventLogger logger,
        [Description("The integer ID of the original product.")] int productId)
    {
        return logger.ExecuteToolAsync("find_related_products", new { productId }, async () =>
        {
            var outcome = await service.GetRelatedProductsByIdAsync(productId);
            return ResponseFormatter.FormatOutcome(outcome, list => 
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"Found {list.Count} related products:");
                foreach (var item in list) sb.AppendLine("- " + ResponseFormatter.FormatProductSummary(item));
                return sb.ToString().TrimEnd();
            });
        });
    }

    [McpServerTool(Name = "find_related_by_slug")]
    [Description("Finds products that are related to a given product slug (usually same category).")]
    public static Task<string> FindRelatedBySlug(
        ICatalogQueryService service,
        StructuredEventLogger logger,
        [Description("The string slug of the original product.")] string slug)
    {
        return logger.ExecuteToolAsync("find_related_by_slug", new { slug }, async () =>
        {
            var outcome = await service.GetRelatedProductsBySlugAsync(slug);
            return ResponseFormatter.FormatOutcome(outcome, list => 
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"Found {list.Count} related products:");
                foreach (var item in list) sb.AppendLine("- " + ResponseFormatter.FormatProductSummary(item));
                return sb.ToString().TrimEnd();
            });
        });
    }

    [McpServerTool(Name = "filter_store_products")]
    [Description("Searches and filters products by various criteria like title, price range, and category.")]
    public static Task<string> FilterStoreProducts(
        ICatalogQueryService service,
        StructuredEventLogger logger,
        [Description("Optional substring to match in product title.")] string? title = null,
        [Description("Optional minimum price boundary.")] decimal? priceMin = null,
        [Description("Optional maximum price boundary.")] decimal? priceMax = null,
        [Description("Optional category ID to restrict results to.")] int? categoryId = null,
        [Description("Optional category slug to restrict results to.")] string? categorySlug = null,
        [Description("Number of items to skip. Defaults to 0.")] int? offset = null,
        [Description("Maximum number of items to return. Defaults to 10.")] int? limit = null)
    {
        return logger.ExecuteToolAsync("filter_store_products", new { title, priceMin, priceMax, categoryId, categorySlug, offset, limit }, async () =>
        {
            var criteria = new CatalogFilterCriteria
            {
                Title = title,
                PriceMin = priceMin,
                PriceMax = priceMax,
                CategoryId = categoryId,
                CategorySlug = categorySlug,
                Offset = offset,
                Limit = limit
            };
            var outcome = await service.FilterProductsAsync(criteria);
            return ResponseFormatter.FormatOutcome(outcome, ResponseFormatter.FormatProductList);
        });
    }
}
