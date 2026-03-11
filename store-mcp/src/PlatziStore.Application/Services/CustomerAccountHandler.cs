using PlatziStore.Application.Contracts;
using PlatziStore.Application.DataTransfer;
using PlatziStore.Application.Mapping;
using PlatziStore.Shared.Exceptions;
using PlatziStore.Shared.Models;

namespace PlatziStore.Application.Services;

public class CustomerAccountHandler : ICustomerAccountService
{
    private readonly IStoreGateway _gateway;

    public CustomerAccountHandler(IStoreGateway gateway)
    {
        _gateway = gateway;
    }

    public async Task<OperationOutcome<IReadOnlyList<CustomerProfile>>> ListCustomersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var users = await _gateway.GetAllUsersAsync(cancellationToken);
            var profiles = users.Select(EntityMapper.ToProfile).ToList();
            return OperationOutcome<IReadOnlyList<CustomerProfile>>.Success(profiles);
        }
        catch (ExternalServiceException ex)
        {
            return OperationOutcome<IReadOnlyList<CustomerProfile>>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return OperationOutcome<IReadOnlyList<CustomerProfile>>.Failure($"An unexpected error occurred: {ex.Message}");
        }
    }

    public async Task<OperationOutcome<CustomerProfile>> GetCustomerByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _gateway.GetUserByIdAsync(id, cancellationToken);
            return OperationOutcome<CustomerProfile>.Success(EntityMapper.ToProfile(user));
        }
        catch (Exception ex) when (ex.GetType().Name == "EntityNotFoundException")
        {
            return OperationOutcome<CustomerProfile>.Failure($"User with ID {id} was not found.");
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

    public async Task<OperationOutcome<CustomerProfile>> RegisterCustomerAsync(CustomerRegistration request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return OperationOutcome<CustomerProfile>.Failure("Name is required.");
            
        if (string.IsNullOrWhiteSpace(request.Email))
            return OperationOutcome<CustomerProfile>.Failure("Email is required.");
            
        if (string.IsNullOrWhiteSpace(request.Password))
            return OperationOutcome<CustomerProfile>.Failure("Password is required.");

        try
        {
            var user = await _gateway.CreateUserAsync(request, cancellationToken);
            return OperationOutcome<CustomerProfile>.Success(EntityMapper.ToProfile(user));
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

    public async Task<OperationOutcome<CustomerProfile>> UpdateCustomerProfileAsync(int id, CustomerRegistration request, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            return OperationOutcome<CustomerProfile>.Failure("Invalid user ID.");

        if (string.IsNullOrWhiteSpace(request.Name))
            return OperationOutcome<CustomerProfile>.Failure("Name is required.");
            
        if (string.IsNullOrWhiteSpace(request.Email))
            return OperationOutcome<CustomerProfile>.Failure("Email is required.");
            
        if (string.IsNullOrWhiteSpace(request.Password))
            return OperationOutcome<CustomerProfile>.Failure("Password is required.");

        try
        {
            var user = await _gateway.UpdateUserAsync(id, request, cancellationToken);
            return OperationOutcome<CustomerProfile>.Success(EntityMapper.ToProfile(user));
        }
        catch (Exception ex) when (ex.GetType().Name == "EntityNotFoundException")
        {
            return OperationOutcome<CustomerProfile>.Failure($"User with ID {id} was not found.");
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

    public async Task<OperationOutcome<bool>> CheckEmailAvailabilityAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return OperationOutcome<bool>.Failure("Email is required.");

        try
        {
            var isAvailable = await _gateway.CheckEmailAvailabilityAsync(email, cancellationToken);
            return OperationOutcome<bool>.Success(isAvailable);
        }
        catch (ExternalServiceException ex)
        {
            return OperationOutcome<bool>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return OperationOutcome<bool>.Failure($"An unexpected error occurred: {ex.Message}");
        }
    }
}
