using PlatziStore.Domain.ValueObjects;

namespace PlatziStore.Domain.Entities;

public sealed class ProductGroup
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public SlugIdentifier Slug { get; init; } = null!;

    public ImageUrl CoverImage { get; init; } = null!;
}

