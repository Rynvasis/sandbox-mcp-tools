using PlatziStore.Application.DataTransfer;
using PlatziStore.Shared.Models;

namespace PlatziStore.Application.Contracts;

public interface ICatalogCommandService
{
    Task<OperationOutcome<CatalogItemDetail>> CreateProductAsync(CatalogItemPayload payload, CancellationToken cancellationToken = default);
    Task<OperationOutcome<CatalogItemDetail>> UpdateProductAsync(int id, CatalogItemPayload payload, CancellationToken cancellationToken = default);
    Task<OperationOutcome<bool>> DeleteProductAsync(int id, CancellationToken cancellationToken = default);
}
