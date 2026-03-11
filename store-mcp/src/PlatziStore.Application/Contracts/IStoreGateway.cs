using PlatziStore.Domain.Entities;
using PlatziStore.Application.DataTransfer; // For AuthTokenPair

namespace PlatziStore.Application.Contracts;

public interface IStoreGateway
{
    // Products
    Task<IReadOnlyList<Merchandise>> GetAllProductsAsync(int? offset = null, int? limit = null, CancellationToken cancellationToken = default);
    Task<Merchandise> GetProductByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Merchandise> GetProductBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Merchandise>> GetRelatedProductsByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Merchandise>> GetRelatedProductsBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Merchandise>> FilterProductsAsync(string? title = null, decimal? priceMin = null, decimal? priceMax = null, int? categoryId = null, string? categorySlug = null, int? offset = null, int? limit = null, CancellationToken cancellationToken = default);
    
    // Command requests for Products need primitive payloads from handlers to avoid Infrastructure dependencies
    // Alternatively, handlers can map to anonymous objects or primitive types, but simplest is taking primitive arguments
    // To match IPlatziStoreGateway while staying clean, we redefine the request DTOs here or pass primitives.
    // The Gateway interface provided by Infrastructure accepts CreateProductApiRequest. 
    // For IStoreGateway, we take Application DTOs and the Host layer configures a decorator/adapter to call IPlatziStoreGateway.
    
    Task<Merchandise> CreateProductAsync(CatalogItemPayload request, CancellationToken cancellationToken = default);
    Task<Merchandise> UpdateProductAsync(int id, CatalogItemPayload request, CancellationToken cancellationToken = default);
    Task<bool> DeleteProductAsync(int id, CancellationToken cancellationToken = default);

    // Categories
    Task<IReadOnlyList<ProductGroup>> GetAllCategoriesAsync(CancellationToken cancellationToken = default);
    Task<ProductGroup> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ProductGroup> GetCategoryBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Merchandise>> GetProductsByCategoryAsync(int categoryId, int? offset = null, int? limit = null, CancellationToken cancellationToken = default);
    
    Task<ProductGroup> CreateCategoryAsync(CategoryPayload request, CancellationToken cancellationToken = default);
    Task<ProductGroup> UpdateCategoryAsync(int id, CategoryPayload request, CancellationToken cancellationToken = default);
    Task<bool> DeleteCategoryAsync(int id, CancellationToken cancellationToken = default);

    // Users
    Task<IReadOnlyList<StoreCustomer>> GetAllUsersAsync(CancellationToken cancellationToken = default);
    Task<StoreCustomer> GetUserByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<StoreCustomer> CreateUserAsync(CustomerRegistration request, CancellationToken cancellationToken = default);
    Task<StoreCustomer> UpdateUserAsync(int id, CustomerRegistration request, CancellationToken cancellationToken = default);
    Task<bool> CheckEmailAvailabilityAsync(string email, CancellationToken cancellationToken = default);

    // Auth
    Task<AuthTokenPair> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<StoreCustomer> GetProfileAsync(string accessToken, CancellationToken cancellationToken = default);
    Task<AuthTokenPair> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
}
