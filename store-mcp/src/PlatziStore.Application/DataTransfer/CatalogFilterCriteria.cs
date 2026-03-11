namespace PlatziStore.Application.DataTransfer;

public record CatalogFilterCriteria
{
    public string? Title { get; init; }
    public decimal? PriceMin { get; init; }
    public decimal? PriceMax { get; init; }
    public int? CategoryId { get; init; }
    public string? CategorySlug { get; init; }
    public int? Offset { get; init; }
    public int? Limit { get; init; }
}
