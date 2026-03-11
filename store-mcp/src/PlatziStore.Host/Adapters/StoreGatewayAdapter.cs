using PlatziStore.Application.Contracts;
using PlatziStore.Application.DataTransfer;
using PlatziStore.Domain.Entities;
using PlatziStore.Infrastructure.ApiClients;
using PlatziStore.Infrastructure.ApiClients.Dtos;

namespace PlatziStore.Host.Adapters;

public class StoreGatewayAdapter : IStoreGateway
{
    private readonly IPlatziStoreGateway _gateway;

    public StoreGatewayAdapter(IPlatziStoreGateway gateway)
    {
        _gateway = gateway;
    }

    public Task<bool> CheckEmailAvailabilityAsync(string email, CancellationToken cancellationToken = default) =>
        _gateway.CheckEmailAvailabilityAsync(email, cancellationToken);

    public Task<ProductGroup> CreateCategoryAsync(CategoryPayload request, CancellationToken cancellationToken = default) =>
        _gateway.CreateCategoryAsync(new CreateCategoryApiRequest
        {
            Name = request.Name,
            Image = request.Image
        }, cancellationToken);

    public Task<Merchandise> CreateProductAsync(CatalogItemPayload request, CancellationToken cancellationToken = default) =>
        _gateway.CreateProductAsync(new CreateProductApiRequest
        {
            Title = request.Title,
            Price = (int)request.Price,
            Description = request.Description,
            CategoryId = request.CategoryId,
            Images = request.Images.ToList()
        }, cancellationToken);

    public Task<StoreCustomer> CreateUserAsync(CustomerRegistration request, CancellationToken cancellationToken = default) =>
        _gateway.CreateUserAsync(new CreateUserApiRequest
        {
            Name = request.Name,
            Email = request.Email,
            Password = request.Password,
            Avatar = request.Avatar
        }, cancellationToken);

    public Task<bool> DeleteCategoryAsync(int id, CancellationToken cancellationToken = default) =>
        _gateway.DeleteCategoryAsync(id, cancellationToken);

    public Task<bool> DeleteProductAsync(int id, CancellationToken cancellationToken = default) =>
        _gateway.DeleteProductAsync(id, cancellationToken);

    public Task<IReadOnlyList<ProductGroup>> GetAllCategoriesAsync(CancellationToken cancellationToken = default) =>
        _gateway.GetAllCategoriesAsync(cancellationToken);

    public Task<IReadOnlyList<Merchandise>> GetAllProductsAsync(int? offset = null, int? limit = null, CancellationToken cancellationToken = default) =>
        _gateway.GetAllProductsAsync(offset, limit, cancellationToken);

    public Task<IReadOnlyList<StoreCustomer>> GetAllUsersAsync(CancellationToken cancellationToken = default) =>
        _gateway.GetAllUsersAsync(cancellationToken);

    public Task<ProductGroup> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _gateway.GetCategoryByIdAsync(id, cancellationToken);

    public Task<ProductGroup> GetCategoryBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        _gateway.GetCategoryBySlugAsync(slug, cancellationToken);

    public Task<Merchandise> GetProductByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _gateway.GetProductByIdAsync(id, cancellationToken);

    public Task<Merchandise> GetProductBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        _gateway.GetProductBySlugAsync(slug, cancellationToken);

    public Task<IReadOnlyList<Merchandise>> GetProductsByCategoryAsync(int categoryId, int? offset = null, int? limit = null, CancellationToken cancellationToken = default) =>
        _gateway.GetProductsByCategoryAsync(categoryId, offset, limit, cancellationToken);

    public Task<StoreCustomer> GetProfileAsync(string accessToken, CancellationToken cancellationToken = default) =>
        _gateway.GetProfileAsync(accessToken, cancellationToken);

    public Task<IReadOnlyList<Merchandise>> GetRelatedProductsByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _gateway.GetRelatedProductsByIdAsync(id, cancellationToken);

    public Task<IReadOnlyList<Merchandise>> GetRelatedProductsBySlugAsync(string slug, CancellationToken cancellationToken = default) =>
        _gateway.GetRelatedProductsBySlugAsync(slug, cancellationToken);

    public Task<StoreCustomer> GetUserByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _gateway.GetUserByIdAsync(id, cancellationToken);

    public async Task<AuthTokenPair> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var result = await _gateway.LoginAsync(email, password, cancellationToken);
        return new AuthTokenPair
        {
            AccessToken = result.AccessToken,
            RefreshToken = result.RefreshToken
        };
    }

    public async Task<AuthTokenPair> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var result = await _gateway.RefreshTokenAsync(refreshToken, cancellationToken);
        return new AuthTokenPair
        {
            AccessToken = result.AccessToken,
            RefreshToken = result.RefreshToken
        };
    }

    public Task<ProductGroup> UpdateCategoryAsync(int id, CategoryPayload request, CancellationToken cancellationToken = default) =>
        _gateway.UpdateCategoryAsync(id, new UpdateCategoryApiRequest
        {
            Name = request.Name,
            Image = request.Image
        }, cancellationToken);

    public Task<Merchandise> UpdateProductAsync(int id, CatalogItemPayload request, CancellationToken cancellationToken = default) =>
        _gateway.UpdateProductAsync(id, new UpdateProductApiRequest
        {
            Title = request.Title,
            Price = (int)request.Price,
            Description = request.Description,
            CategoryId = request.CategoryId,
            Images = request.Images.ToList()
        }, cancellationToken);

    public Task<StoreCustomer> UpdateUserAsync(int id, CustomerRegistration request, CancellationToken cancellationToken = default) =>
        _gateway.UpdateUserAsync(id, new UpdateUserApiRequest
        {
            Name = string.IsNullOrEmpty(request.Name) ? null : request.Name,
            Email = string.IsNullOrEmpty(request.Email) ? null : request.Email,
            Password = string.IsNullOrEmpty(request.Password) ? null : request.Password,
            Avatar = string.IsNullOrEmpty(request.Avatar) ? null : request.Avatar
        }, cancellationToken);

    public Task<IReadOnlyList<Merchandise>> FilterProductsAsync(string? title = null, decimal? priceMin = null, decimal? priceMax = null, int? categoryId = null, string? categorySlug = null, int? offset = null, int? limit = null, CancellationToken cancellationToken = default) =>
        _gateway.FilterProductsAsync(title, (int?)priceMin, (int?)priceMax, categoryId, categorySlug, offset, limit, cancellationToken);
}
