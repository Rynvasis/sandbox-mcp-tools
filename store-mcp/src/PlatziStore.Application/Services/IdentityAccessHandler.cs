using PlatziStore.Application.Contracts;
using PlatziStore.Application.DataTransfer;
using PlatziStore.Application.Mapping;
using PlatziStore.Shared.Exceptions;
using PlatziStore.Shared.Models;

namespace PlatziStore.Application.Services;

public class IdentityAccessHandler : IIdentityAccessService
{
    private readonly IStoreGateway _gateway;

    public IdentityAccessHandler(IStoreGateway gateway)
    {
        _gateway = gateway;
    }

    public async Task<OperationOutcome<AuthTokenPair>> AuthenticateAsync(AuthCredentials credentials, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(credentials.Email))
            return OperationOutcome<AuthTokenPair>.Failure("Email is required.");
            
        if (string.IsNullOrWhiteSpace(credentials.Password))
            return OperationOutcome<AuthTokenPair>.Failure("Password is required.");

        try
        {
            var tokens = await _gateway.LoginAsync(credentials.Email, credentials.Password, cancellationToken);
            return OperationOutcome<AuthTokenPair>.Success(tokens);
        }
        catch (Exception ex) when (ex.GetType().Name == "AuthorizationDeniedException")
        {
            return OperationOutcome<AuthTokenPair>.Failure("Invalid email or password.");
        }
        catch (ExternalServiceException ex)
        {
            return OperationOutcome<AuthTokenPair>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return OperationOutcome<AuthTokenPair>.Failure($"An unexpected error occurred: {ex.Message}");
        }
    }

    public async Task<OperationOutcome<CustomerProfile>> GetAuthenticatedProfileAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
            return OperationOutcome<CustomerProfile>.Failure("Access token is required.");

        try
        {
            var user = await _gateway.GetProfileAsync(accessToken, cancellationToken);
            return OperationOutcome<CustomerProfile>.Success(EntityMapper.ToProfile(user));
        }
        catch (Exception ex) when (ex.GetType().Name == "AuthorizationDeniedException")
        {
            return OperationOutcome<CustomerProfile>.Failure("Invalid or expired access token.");
        }
        catch (ExternalServiceException ex)
        {
            return OperationOutcome<CustomerProfile>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return OperationOutcome<CustomerProfile>.Failure($"An unexpected error occurred: {ex.Message}");
        }
    }

    public async Task<OperationOutcome<AuthTokenPair>> RefreshSessionAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return OperationOutcome<AuthTokenPair>.Failure("Refresh token is required.");

        try
        {
            var tokens = await _gateway.RefreshTokenAsync(refreshToken, cancellationToken);
            return OperationOutcome<AuthTokenPair>.Success(tokens);
        }
        catch (Exception ex) when (ex.GetType().Name == "AuthorizationDeniedException")
        {
            return OperationOutcome<AuthTokenPair>.Failure("Invalid or expired refresh token.");
        }
        catch (ExternalServiceException ex)
        {
            return OperationOutcome<AuthTokenPair>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return OperationOutcome<AuthTokenPair>.Failure($"An unexpected error occurred: {ex.Message}");
        }
    }
}
