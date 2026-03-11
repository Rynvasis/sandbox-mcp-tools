namespace PlatziStore.Application.DataTransfer;

public record AuthTokenPair
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
}
