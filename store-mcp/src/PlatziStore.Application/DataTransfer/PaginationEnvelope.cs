namespace PlatziStore.Application.DataTransfer;

public record PaginationEnvelope
{
    public int? Offset { get; init; }
    public int? Limit { get; init; }
}
