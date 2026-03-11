using PlatziStore.Application.Contracts;
using PlatziStore.Application.DataTransfer;
using PlatziStore.Application.Mapping;
using PlatziStore.Shared.Exceptions;
using PlatziStore.Shared.Models;

namespace PlatziStore.Application.Services;

public class CatalogQueryHandler : ICatalogQueryService
{
    private readonly IStoreGateway _gateway;

    public CatalogQueryHandler(IStoreGateway gateway)
    {
        _gateway = gateway;
    }

    public async Task<OperationOutcome<PagedCollection<CatalogItemSummary>>> ListProductsAsync(PaginationEnvelope? pagination = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var offset = pagination?.Offset;
            var limit = pagination?.Limit;
            
            var products = await _gateway.GetAllProductsAsync(offset, limit, cancellationToken);
            
            var summaries = products.Select(EntityMapper.ToSummary).ToList();
            var pagedCollection = PagedCollection<CatalogItemSummary>.Create(
                summaries, 
                offset ?? 0, 
                limit ?? summaries.Count);
                
            return OperationOutcome<PagedCollection<CatalogItemSummary>>.Success(pagedCollection);
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

    public async Task<OperationOutcome<CatalogItemDetail>> GetProductByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var product = await _gateway.GetProductByIdAsync(id, cancellationToken);
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

    public async Task<OperationOutcome<CatalogItemDetail>> GetProductBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        try
        {
            var product = await _gateway.GetProductBySlugAsync(slug, cancellationToken);
            return OperationOutcome<CatalogItemDetail>.Success(EntityMapper.ToDetail(product));
        }
        catch (Exception ex) when (ex.GetType().Name == "EntityNotFoundException")
        {
            return OperationOutcome<CatalogItemDetail>.Failure($"Product with slug '{slug}' was not found.");
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

    public async Task<OperationOutcome<PagedCollection<CatalogItemSummary>>> FilterProductsAsync(CatalogFilterCriteria criteria, CancellationToken cancellationToken = default)
    {
        try
        {
            var products = await _gateway.FilterProductsAsync(
                title: criteria.Title,
                priceMin: criteria.PriceMin,
                priceMax: criteria.PriceMax,
                categoryId: criteria.CategoryId,
                categorySlug: criteria.CategorySlug,
                offset: criteria.Offset,
                limit: criteria.Limit,
                cancellationToken: cancellationToken);
                
            var summaries = products.Select(EntityMapper.ToSummary).ToList();
            var pagedCollection = PagedCollection<CatalogItemSummary>.Create(
                summaries, 
                criteria.Offset ?? 0, 
                criteria.Limit ?? summaries.Count);
                
            return OperationOutcome<PagedCollection<CatalogItemSummary>>.Success(pagedCollection);
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

    public async Task<OperationOutcome<IReadOnlyList<CatalogItemSummary>>> GetRelatedProductsByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var products = await _gateway.GetRelatedProductsByIdAsync(id, cancellationToken);
            var summaries = products.Select(EntityMapper.ToSummary).ToList();
            return OperationOutcome<IReadOnlyList<CatalogItemSummary>>.Success(summaries);
        }
        catch (Exception ex) when (ex.GetType().Name == "EntityNotFoundException")
        {
            return OperationOutcome<IReadOnlyList<CatalogItemSummary>>.Failure($"Product with ID {id} was not found.");
        }
        catch (ExternalServiceException ex)
        {
            return OperationOutcome<IReadOnlyList<CatalogItemSummary>>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return OperationOutcome<IReadOnlyList<CatalogItemSummary>>.Failure($"An unexpected error occurred: {ex.Message}");
        }
    }

    public async Task<OperationOutcome<IReadOnlyList<CatalogItemSummary>>> GetRelatedProductsBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        try
        {
            var products = await _gateway.GetRelatedProductsBySlugAsync(slug, cancellationToken);
            var summaries = products.Select(EntityMapper.ToSummary).ToList();
            return OperationOutcome<IReadOnlyList<CatalogItemSummary>>.Success(summaries);
        }
        catch (Exception ex) when (ex.GetType().Name == "EntityNotFoundException")
        {
            return OperationOutcome<IReadOnlyList<CatalogItemSummary>>.Failure($"Product with slug '{slug}' was not found.");
        }
        catch (ExternalServiceException ex)
        {
            return OperationOutcome<IReadOnlyList<CatalogItemSummary>>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return OperationOutcome<IReadOnlyList<CatalogItemSummary>>.Failure($"An unexpected error occurred: {ex.Message}");
        }
    }
}
