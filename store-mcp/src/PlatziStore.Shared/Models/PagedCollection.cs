namespace PlatziStore.Shared.Models;

public record PagedCollection<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int Offset { get; init; }
    public int Limit { get; init; }
    public int Total { get; init; }

    private PagedCollection() { }

    public static PagedCollection<T> Create(IReadOnlyList<T> items, int offset, int limit, int total = -1) => new()
    {
        Items = items,
        Offset = offset,
        Limit = limit,
        Total = total
    };
}
