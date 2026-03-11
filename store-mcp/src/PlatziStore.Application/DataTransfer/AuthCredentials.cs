namespace PlatziStore.Application.DataTransfer;

public record AuthCredentials
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
