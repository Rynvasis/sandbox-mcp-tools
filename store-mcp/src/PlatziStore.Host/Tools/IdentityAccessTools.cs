using System.ComponentModel;
using ModelContextProtocol.Server;
using PlatziStore.Application.Contracts;
using PlatziStore.Application.DataTransfer;
using PlatziStore.Host.Formatting;
using PlatziStore.Infrastructure.Observability;

namespace PlatziStore.Host.Tools;

[McpServerToolType]
public static class IdentityAccessTools
{
    [McpServerTool(Name = "authenticate_customer")]
    [Description("Authenticates a customer and returns an access token / refresh token pair.")]
    public static Task<string> AuthenticateCustomer(
        IIdentityAccessService service,
        StructuredEventLogger logger,
        [Description("The email address of the customer.")] string email,
        [Description("The plaintext password.")] string password)
    {
        return logger.ExecuteToolAsync("authenticate_customer", new { email, password }, async () =>
        {
            var credentials = new AuthCredentials { Email = email, Password = password };
            var outcome = await service.AuthenticateAsync(credentials);
            return ResponseFormatter.FormatOutcome(outcome, ResponseFormatter.FormatTokenPair);
        });
    }

    [McpServerTool(Name = "get_authenticated_profile")]
    [Description("Retrieves the customer profile associated with a valid access token.")]
    public static Task<string> GetAuthenticatedProfile(
        IIdentityAccessService service,
        StructuredEventLogger logger,
        [Description("The JWT access token.")] string accessToken)
    {
        return logger.ExecuteToolAsync("get_authenticated_profile", new { accessToken }, async () =>
        {
            var outcome = await service.GetAuthenticatedProfileAsync(accessToken);
            return ResponseFormatter.FormatOutcome(outcome, ResponseFormatter.FormatCustomerProfile);
        });
    }

    [McpServerTool(Name = "refresh_access_token")]
    [Description("Refreshes an expired access token using a valid refresh token.")]
    public static Task<string> RefreshAccessToken(
        IIdentityAccessService service,
        StructuredEventLogger logger,
        [Description("The valid refresh token.")] string refreshToken)
    {
        return logger.ExecuteToolAsync("refresh_access_token", new { refreshToken }, async () =>
        {
            var outcome = await service.RefreshSessionAsync(refreshToken);
            return ResponseFormatter.FormatOutcome(outcome, ResponseFormatter.FormatTokenPair);
        });
    }
}
