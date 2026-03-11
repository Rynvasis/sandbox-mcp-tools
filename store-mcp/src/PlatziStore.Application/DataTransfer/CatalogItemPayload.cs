namespace PlatziStore.Application.DataTransfer;

public record CatalogItemPayload
{
    public string Title { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string Description { get; init; } = string.Empty;
    public int CategoryId { get; init; }
    public IReadOnlyList<string> Images { get; init; } = Array.Empty<string>();
}
