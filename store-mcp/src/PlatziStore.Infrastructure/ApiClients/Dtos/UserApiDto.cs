namespace PlatziStore.Infrastructure.ApiClients.Dtos;

public class UserApiDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Avatar { get; set; } = string.Empty;
    public string? CreationAt { get; set; }
    public string? UpdatedAt { get; set; }
}
