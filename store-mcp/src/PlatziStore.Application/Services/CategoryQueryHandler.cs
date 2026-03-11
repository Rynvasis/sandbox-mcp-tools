using PlatziStore.Application.Contracts;
using PlatziStore.Application.DataTransfer;
using PlatziStore.Application.Mapping;
using PlatziStore.Shared.Exceptions;
using PlatziStore.Shared.Models;

namespace PlatziStore.Application.Services;

public class CategoryQueryHandler : ICategoryQueryService
{
    private readonly IStoreGateway _gateway;

    public CategoryQueryHandler(IStoreGateway gateway)
    {
        _gateway = gateway;
    }

    public async Task<OperationOutcome<IReadOnlyList<CategorySummary>>> ListCategoriesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var categories = await _gateway.GetAllCategoriesAsync(cancellationToken);
            var summaries = categories.Select(EntityMapper.ToSummary).ToList();
            return OperationOutcome<IReadOnlyList<CategorySummary>>.Success(summaries);
        }
        catch (ExternalServiceException ex)
        {
            return OperationOutcome<IReadOnlyList<CategorySummary>>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return OperationOutcome<IReadOnlyList<CategorySummary>>.Failure($"An unexpected error occurred: {ex.Message}");
        }
    }

    public async Task<OperationOutcome<CategorySummary>> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var category = await _gateway.GetCategoryByIdAsync(id, cancellationToken);
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

    public async Task<OperationOutcome<CategorySummary>> GetCategoryBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        try
        {
            var category = await _gateway.GetCategoryBySlugAsync(slug, cancellationToken);
            return OperationOutcome<CategorySummary>.Success(EntityMapper.ToSummary(category));
        }
        catch (Exception ex) when (ex.GetType().Name == "EntityNotFoundException")
        {
            return OperationOutcome<CategorySummary>.Failure($"Category with slug '{slug}' was not found.");
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

    public async Task<OperationOutcome<PagedCollection<CatalogItemSummary>>> ListProductsByCategoryAsync(int categoryId, PaginationEnvelope? pagination = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var offset = pagination?.Offset;
            var limit = pagination?.Limit;
            
            var products = await _gateway.GetProductsByCategoryAsync(categoryId, offset, limit, cancellationToken);
            
            var summaries = products.Select(EntityMapper.ToSummary).ToList();
            var pagedCollection = PagedCollection<CatalogItemSummary>.Create(
                summaries, 
                offset ?? 0, 
                limit ?? summaries.Count);
                
            return OperationOutcome<PagedCollection<CatalogItemSummary>>.Success(pagedCollection);
        }
        catch (Exception ex) when (ex.GetType().Name == "EntityNotFoundException")
        {
            return OperationOutcome<PagedCollection<CatalogItemSummary>>.Failure($"Category with ID {categoryId} was not found.");
        }
        catch (ExternalServiceException ex)
        {
            return OperationOutcome<PagedCollection<CatalogItemSummary>>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return OperationOutcome<PagedCollection<CatalogItemSummary>>.Failure($"An unexpected error occurred: {ex.Message}");
        }
    }
}
