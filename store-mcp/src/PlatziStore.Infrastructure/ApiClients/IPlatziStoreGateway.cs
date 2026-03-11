using PlatziStore.Domain.Entities;
using PlatziStore.Infrastructure.ApiClients.Dtos;

namespace PlatziStore.Infrastructure.ApiClients;

public interface IPlatziStoreGateway
{
    // Products
    Task<IReadOnlyList<Merchandise>> GetAllProductsAsync(int? offset = null, int? limit = null, CancellationToken cancellationToken = default);
    Task<Merchandise> GetProductByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Merchandise> GetProductBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Merchandise>> GetRelatedProductsByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Merchandise>> GetRelatedProductsBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Merchandise>> FilterProductsAsync(string? title = null, decimal? priceMin = null, decimal? priceMax = null, int? categoryId = null, string? categorySlug = null, int? offset = null, int? limit = null, CancellationToken cancellationToken = default);
    
    Task<Merchandise> CreateProductAsync(CreateProductApiRequest request, CancellationToken cancellationToken = default);
    Task<Merchandise> UpdateProductAsync(int id, UpdateProductApiRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteProductAsync(int id, CancellationToken cancellationToken = default);

    // Categories
    Task<IReadOnlyList<ProductGroup>> GetAllCategoriesAsync(CancellationToken cancellationToken = default);
    Task<ProductGroup> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ProductGroup> GetCategoryBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Merchandise>> GetProductsByCategoryAsync(int categoryId, int? offset = null, int? limit = null, CancellationToken cancellationToken = default);
    
    Task<ProductGroup> CreateCategoryAsync(CreateCategoryApiRequest request, CancellationToken cancellationToken = default);
    Task<ProductGroup> UpdateCategoryAsync(int id, UpdateCategoryApiRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteCategoryAsync(int id, CancellationToken cancellationToken = default);

    // Users
    Task<IReadOnlyList<StoreCustomer>> GetAllUsersAsync(CancellationToken cancellationToken = default);
    Task<StoreCustomer> GetUserByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<StoreCustomer> CreateUserAsync(CreateUserApiRequest request, CancellationToken cancellationToken = default);
    Task<StoreCustomer> UpdateUserAsync(int id, UpdateUserApiRequest request, CancellationToken cancellationToken = default);
    Task<bool> CheckEmailAvailabilityAsync(string email, CancellationToken cancellationToken = default);

    // Auth
    Task<AuthTokensApiDto> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<StoreCustomer> GetProfileAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<AuthTokensApiDto> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}
