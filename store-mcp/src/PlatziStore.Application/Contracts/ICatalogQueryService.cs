using PlatziStore.Application.DataTransfer;
using PlatziStore.Shared.Models;

namespace PlatziStore.Application.Contracts;

public interface ICatalogQueryService
{
    Task<OperationOutcome<PagedCollection<CatalogItemSummary>>> ListProductsAsync(PaginationEnvelope? pagination = null, CancellationToken cancellationToken = default);
    Task<OperationOutcome<CatalogItemDetail>> GetProductByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<OperationOutcome<CatalogItemDetail>> GetProductBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<OperationOutcome<PagedCollection<CatalogItemSummary>>> FilterProductsAsync(CatalogFilterCriteria criteria, CancellationToken cancellationToken = default);
    Task<OperationOutcome<IReadOnlyList<CatalogItemSummary>>> GetRelatedProductsByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<OperationOutcome<IReadOnlyList<CatalogItemSummary>>> GetRelatedProductsBySlugAsync(string slug, CancellationToken cancellationToken = default);
}
