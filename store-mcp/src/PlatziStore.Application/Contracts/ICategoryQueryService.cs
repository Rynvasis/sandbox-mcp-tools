using PlatziStore.Application.DataTransfer;
using PlatziStore.Shared.Models;

namespace PlatziStore.Application.Contracts;

public interface ICategoryQueryService
{
    Task<OperationOutcome<IReadOnlyList<CategorySummary>>> ListCategoriesAsync(CancellationToken cancellationToken = default);
    Task<OperationOutcome<CategorySummary>> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<OperationOutcome<CategorySummary>> GetCategoryBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<OperationOutcome<PagedCollection<CatalogItemSummary>>> ListProductsByCategoryAsync(int categoryId, PaginationEnvelope? pagination = null, CancellationToken cancellationToken = default);
}
