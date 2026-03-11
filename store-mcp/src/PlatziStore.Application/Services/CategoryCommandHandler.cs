using PlatziStore.Application.Contracts;
using PlatziStore.Application.DataTransfer;
using PlatziStore.Application.Mapping;
using PlatziStore.Shared.Exceptions;
using PlatziStore.Shared.Models;

namespace PlatziStore.Application.Services;

public class CategoryCommandHandler : ICategoryCommandService
{
    private readonly IStoreGateway _gateway;

    public CategoryCommandHandler(IStoreGateway gateway)
    {
        _gateway = gateway;
    }

    public async Task<OperationOutcome<CategorySummary>> CreateCategoryAsync(CategoryPayload payload, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(payload.Name))
            return OperationOutcome<CategorySummary>.Failure("Name is required.");
            
        if (string.IsNullOrWhiteSpace(payload.Image))
            return OperationOutcome<CategorySummary>.Failure("Image URL is required.");

        try
        {
            var category = await _gateway.CreateCategoryAsync(payload, cancellationToken);
            return OperationOutcome<CategorySummary>.Success(EntityMapper.ToSummary(category));
        }
        catch (ExternalServiceException ex)
        {
            return OperationOutcome<CategorySummary>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return OperationOutcome<CategorySummary>.Failure($"An unexpected error occurred: {ex.Message}");
        }
    }

    public async Task<OperationOutcome<CategorySummary>> UpdateCategoryAsync(int id, CategoryPayload payload, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            return OperationOutcome<CategorySummary>.Failure("Invalid category ID.");

        if (string.IsNullOrWhiteSpace(payload.Name))
            return OperationOutcome<CategorySummary>.Failure("Name is required.");
            
        if (string.IsNullOrWhiteSpace(payload.Image))
            return OperationOutcome<CategorySummary>.Failure("Image URL is required.");

        try
        {
            var category = await _gateway.UpdateCategoryAsync(id, payload, cancellationToken);
            return OperationOutcome<CategorySummary>.Success(EntityMapper.ToSummary(category));
        }
        catch (Exception ex) when (ex.GetType().Name == "EntityNotFoundException")
        {
            return OperationOutcome<CategorySummary>.Failure($"Category with ID {id} was not found.");
        }
        catch (ExternalServiceException ex)
        {
            return OperationOutcome<CategorySummary>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return OperationOutcome<CategorySummary>.Failure($"An unexpected error occurred: {ex.Message}");
        }
    }

    public async Task<OperationOutcome<bool>> DeleteCategoryAsync(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            return OperationOutcome<bool>.Failure("Invalid category ID.");

        try
        {
            var result = await _gateway.DeleteCategoryAsync(id, cancellationToken);
            return OperationOutcome<bool>.Success(result);
        }
        catch (Exception ex) when (ex.GetType().Name == "EntityNotFoundException")
        {
            return OperationOutcome<bool>.Failure($"Category with ID {id} was not found.");
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
