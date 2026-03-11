namespace PlatziStore.Infrastructure.ApiClients.Dtos;

public class UpdateUserApiRequest
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? Avatar { get; set; }
}
