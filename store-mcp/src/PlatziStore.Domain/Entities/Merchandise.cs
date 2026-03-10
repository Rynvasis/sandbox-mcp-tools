using System.Collections.Generic;
using PlatziStore.Domain.ValueObjects;

namespace PlatziStore.Domain.Entities;

public sealed class Merchandise
{
    public int Id { get; init; }

    public string Title { get; init; } = string.Empty;

    public SlugIdentifier Slug { get; init; } = null!;

    public MonetaryAmount Price { get; init; } = null!;

    public string Description { get; init; } = string.Empty;

    public ProductGroup Category { get; init; } = null!;

    public IReadOnlyList<ImageUrl> Images { get; init; } = new List<ImageUrl>();
}

