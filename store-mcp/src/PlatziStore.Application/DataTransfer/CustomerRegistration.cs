namespace PlatziStore.Application.DataTransfer;

public record CustomerRegistration
{
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string Avatar { get; init; } = string.Empty;
}
