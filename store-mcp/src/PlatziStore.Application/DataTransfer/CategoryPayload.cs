namespace PlatziStore.Application.DataTransfer;

public record CategoryPayload
{
    public string Name { get; init; } = string.Empty;
    public string Image { get; init; } = string.Empty;
}
