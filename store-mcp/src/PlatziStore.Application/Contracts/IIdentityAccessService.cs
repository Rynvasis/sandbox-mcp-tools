using PlatziStore.Application.DataTransfer;
using PlatziStore.Shared.Models;

namespace PlatziStore.Application.Contracts;

public interface IIdentityAccessService
{
    Task<OperationOutcome<AuthTokenPair>> AuthenticateAsync(AuthCredentials credentials, CancellationToken cancellationToken = default);
    Task<OperationOutcome<CustomerProfile>> GetAuthenticatedProfileAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<OperationOutcome<AuthTokenPair>> RefreshSessionAsync(string refreshToken, CancellationToken cancellationToken = default);
}
