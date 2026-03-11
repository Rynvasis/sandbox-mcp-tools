using PlatziStore.Application.Contracts;
using PlatziStore.Application.DataTransfer;
using PlatziStore.Application.Mapping;
using PlatziStore.Shared.Exceptions;
using PlatziStore.Shared.Models;

namespace PlatziStore.Application.Services;

public class CatalogCommandHandler : ICatalogCommandService
{
    private readonly IStoreGateway _gateway;

    public CatalogCommandHandler(IStoreGateway gateway)
    {
        _gateway = gateway;
    }

    public async Task<OperationOutcome<CatalogItemDetail>> CreateProductAsync(CatalogItemPayload payload, CancellationToken cancellationToken = default)
    {
        // Fail-fast validation
        if (string.IsNullOrWhiteSpace(payload.Title))
            return OperationOutcome<CatalogItemDetail>.Failure("Title is required.");
        
        if (payload.Price <= 0)
            return OperationOutcome<CatalogItemDetail>.Failure("Price must be greater than zero.");
            
        if (payload.CategoryId <= 0)
            return OperationOutcome<CatalogItemDetail>.Failure("CategoryId is required.");
            
        if (payload.Images == null || !payload.Images.Any())
            return OperationOutcome<CatalogItemDetail>.Failure("At least one image URL is required.");

        try
        {
            var product = await _gateway.CreateProductAsync(payload, cancellationToken);
            return OperationOutcome<CatalogItemDetail>.Success(EntityMapper.ToDetail(product));
        }
        catch (ExternalServiceException ex)
        {
            return OperationOutcome<CatalogItemDetail>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return OperationOutcome<CatalogItemDetail>.Failure($"An unexpected error occurred: {ex.Message}");
        }
    }

    public async Task<OperationOutcome<CatalogItemDetail>> UpdateProductAsync(int id, CatalogItemPayload payload, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            return OperationOutcome<CatalogItemDetail>.Failure("Invalid product ID.");

        if (string.IsNullOrWhiteSpace(payload.Title))
            return OperationOutcome<CatalogItemDetail>.Failure("Title is required.");
        
        if (payload.Price <= 0)
            return OperationOutcome<CatalogItemDetail>.Failure("Price must be greater than zero.");

        try
        {
            var product = await _gateway.UpdateProductAsync(id, payload, cancellationToken);
            return OperationOutcome<CatalogItemDetail>.Success(EntityMapper.ToDetail(product));
        }
        catch (Exception ex) when (ex.GetType().Name == "EntityNotFoundException")
        {
            return OperationOutcome<CatalogItemDetail>.Failure($"Product with ID {id} was not found.");
        }
        catch (ExternalServiceException ex)
        {
            return OperationOutcome<CatalogItemDetail>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return OperationOutcome<CatalogItemDetail>.Failure($"An unexpected error occurred: {ex.Message}");
        }
    }

    public async Task<OperationOutcome<bool>> DeleteProductAsync(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            return OperationOutcome<bool>.Failure("Invalid product ID.");

        try
        {
            var result = await _gateway.DeleteProductAsync(id, cancellationToken);
            return OperationOutcome<bool>.Success(result);
        }
        catch (Exception ex) when (ex.GetType().Name == "EntityNotFoundException")
        {
            return OperationOutcome<bool>.Failure($"Product with ID {id} was not found.");
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
