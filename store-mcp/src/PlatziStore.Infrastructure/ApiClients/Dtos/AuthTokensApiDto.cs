namespace PlatziStore.Infrastructure.ApiClients.Dtos;

public class AuthTokensApiDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
