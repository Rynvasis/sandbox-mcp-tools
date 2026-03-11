namespace PlatziStore.Application.DataTransfer;

public record CustomerProfile
{
    public int Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public string AvatarUrl { get; init; } = string.Empty;
}
