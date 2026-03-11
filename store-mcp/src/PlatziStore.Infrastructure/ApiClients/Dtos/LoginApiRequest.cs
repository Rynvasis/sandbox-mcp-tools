namespace PlatziStore.Infrastructure.ApiClients.Dtos;

public class LoginApiRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
