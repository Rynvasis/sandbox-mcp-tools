using PlatziStore.Application.DataTransfer;
using PlatziStore.Shared.Models;

namespace PlatziStore.Application.Contracts;

public interface ICustomerAccountService
{
    Task<OperationOutcome<IReadOnlyList<CustomerProfile>>> ListCustomersAsync(CancellationToken cancellationToken = default);
    Task<OperationOutcome<CustomerProfile>> GetCustomerByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<OperationOutcome<CustomerProfile>> RegisterCustomerAsync(CustomerRegistration request, CancellationToken cancellationToken = default);
    Task<OperationOutcome<CustomerProfile>> UpdateCustomerProfileAsync(int id, CustomerRegistration request, CancellationToken cancellationToken = default);
    Task<OperationOutcome<bool>> CheckEmailAvailabilityAsync(string email, CancellationToken cancellationToken = default);
}
