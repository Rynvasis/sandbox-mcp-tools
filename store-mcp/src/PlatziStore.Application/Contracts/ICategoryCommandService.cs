using PlatziStore.Application.DataTransfer;
using PlatziStore.Shared.Models;

namespace PlatziStore.Application.Contracts;

public interface ICategoryCommandService
{
    Task<OperationOutcome<CategorySummary>> CreateCategoryAsync(CategoryPayload payload, CancellationToken cancellationToken = default);
    Task<OperationOutcome<CategorySummary>> UpdateCategoryAsync(int id, CategoryPayload payload, CancellationToken cancellationToken = default);
    Task<OperationOutcome<bool>> DeleteCategoryAsync(int id, CancellationToken cancellationToken = default);
}
