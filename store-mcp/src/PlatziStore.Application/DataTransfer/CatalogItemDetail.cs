namespace PlatziStore.Application.DataTransfer;

public record CatalogItemDetail
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string Description { get; init; } = string.Empty;
    public int CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public string CategorySlug { get; init; } = string.Empty;
    public IReadOnlyList<string> ImageUrls { get; init; } = Array.Empty<string>();
}
