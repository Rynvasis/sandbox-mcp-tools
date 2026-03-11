using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PlatziStore.Domain.Entities;
using PlatziStore.Domain.Exceptions;
using PlatziStore.Infrastructure.ApiClients.Dtos;
using PlatziStore.Shared.Exceptions;

namespace PlatziStore.Infrastructure.ApiClients;

public class PlatziStoreGateway : IPlatziStoreGateway
{
    private readonly HttpClient _httpClient;
    private readonly PlatziStoreResponseParser _parser;
    private readonly ILogger<PlatziStoreGateway> _logger;

    public PlatziStoreGateway(HttpClient httpClient, PlatziStoreResponseParser parser, ILogger<PlatziStoreGateway> logger)
    {
        _httpClient = httpClient;
        _parser = parser;
        _logger = logger;
    }

    private async Task EnsureSuccessOrThrowAsync(HttpResponseMessage response, string entityType, object? entityId = null, CancellationToken cancellationToken = default)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        string errorContent = string.Empty;
        try
        {
            errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read error content from response.");
        }

        switch (response.StatusCode)
        {
            case HttpStatusCode.NotFound:
                throw new EntityNotFoundException(entityType, entityId ?? "unknown");
            case HttpStatusCode.Unauthorized:
            case HttpStatusCode.Forbidden:
                throw new AuthorizationDeniedException($"Access denied to {entityType} endpoint. Details: {errorContent}");
            default:
                if ((int)response.StatusCode >= 500)
                {
                    throw new ExternalServiceException("PlatziStoreApi", $"Server error: {errorContent}", (int)response.StatusCode);
                }
                throw new ExternalServiceException("PlatziStoreApi", $"Unexpected failure: {response.StatusCode}. Details: {errorContent}", (int)response.StatusCode);
        }
    }

    private async Task<string> SendAndReadStringAsync(HttpRequestMessage request, string entityType, object? entityId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            await EnsureSuccessOrThrowAsync(response, entityType, entityId, cancellationToken);
            return await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException || !cancellationToken.IsCancellationRequested)
        {
            throw new ExternalServiceException("PlatziStoreApi", "The request timed out.", null, ex);
        }
        catch (HttpRequestException ex)
        {
            throw new ExternalServiceException("PlatziStoreApi", $"Network error: {ex.Message}", null, ex);
        }
    }

    // --- PRODUCTS (US1) ---
    public async Task<IReadOnlyList<Merchandise>> GetAllProductsAsync(int? offset = null, int? limit = null, CancellationToken cancellationToken = default)
    {
        var url = PlatziStoreEndpoints.Products;
        if (offset.HasValue && limit.HasValue)
        {
            url += $"?offset={offset.Value}&limit={limit.Value}";
        }
        
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        var json = await SendAndReadStringAsync(request, "Merchandise", null, cancellationToken);
        
        try
        {
            return _parser.ParseProducts(json);
        }
        catch (JsonException ex)
        {
            throw new ExternalServiceException("PlatziStoreApi", "Failed to parse products response.", null, ex);
        }
    }

    public async Task<Merchandise> GetProductByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var url = string.Format(PlatziStoreEndpoints.ProductById, id);
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        var json = await SendAndReadStringAsync(request, "Merchandise", id, cancellationToken);
        
        try
        {
            return _parser.ParseProduct(json);
        }
        catch (JsonException ex)
        {
            throw new ExternalServiceException("PlatziStoreApi", $"Failed to parse product {id} response.", null, ex);
        }
    }

    public async Task<Merchandise> GetProductBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var url = string.Format(PlatziStoreEndpoints.ProductBySlug, slug);
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        var json = await SendAndReadStringAsync(request, "Merchandise", slug, cancellationToken);
        
        try
        {
            return _parser.ParseProduct(json);
        }
        catch (JsonException ex)
        {
            throw new ExternalServiceException("PlatziStoreApi", $"Failed to parse product slug {slug} response.", null, ex);
        }
    }

    public async Task<IReadOnlyList<Merchandise>> GetRelatedProductsByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var url = string.Format(PlatziStoreEndpoints.ProductRelated, id);
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        var json = await SendAndReadStringAsync(request, "Merchandise", id, cancellationToken);
        
        try
        {
            return _parser.ParseProducts(json);
        }
        catch (JsonException ex)
        {
            throw new ExternalServiceException("PlatziStoreApi", "Failed to parse related products response.", null, ex);
        }
    }

    public async Task<IReadOnlyList<Merchandise>> GetRelatedProductsBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var url = string.Format(PlatziStoreEndpoints.ProductRelatedBySlug, slug);
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        var json = await SendAndReadStringAsync(request, "Merchandise", slug, cancellationToken);
        
        try
        {
            return _parser.ParseProducts(json);
        }
        catch (JsonException ex)
        {
            throw new ExternalServiceException("PlatziStoreApi", "Failed to parse related products response.", null, ex);
        }
    }

    public async Task<IReadOnlyList<Merchandise>> FilterProductsAsync(string? title = null, decimal? priceMin = null, decimal? priceMax = null, int? categoryId = null, string? categorySlug = null, int? offset = null, int? limit = null, CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>();
        if (!string.IsNullOrWhiteSpace(title)) queryParams.Add($"title={Uri.EscapeDataString(title)}");
        if (priceMin.HasValue) queryParams.Add($"priceMin={priceMin.Value}");
        if (priceMax.HasValue) queryParams.Add($"priceMax={priceMax.Value}");
        if (categoryId.HasValue) queryParams.Add($"categoryId={categoryId.Value}");
        if (!string.IsNullOrWhiteSpace(categorySlug)) queryParams.Add($"categorySlug={Uri.EscapeDataString(categorySlug)}");
        if (offset.HasValue) queryParams.Add($"offset={offset.Value}");
        if (limit.HasValue) queryParams.Add($"limit={limit.Value}");

        var url = PlatziStoreEndpoints.Products;
        if (queryParams.Any())
        {
            url += "?" + string.Join("&", queryParams);
        }

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        var json = await SendAndReadStringAsync(request, "Merchandise", null, cancellationToken);
        
        try
        {
            return _parser.ParseProducts(json);
        }
        catch (JsonException ex)
        {
            throw new ExternalServiceException("PlatziStoreApi", "Failed to parse filtered products response.", null, ex);
        }
    }

    private async Task<string> SendWithBodyAndReadStringAsync<T>(HttpMethod method, string url, T body, string entityType, object? entityId = null, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(method, url)
        {
            Content = JsonContent.Create(body, null, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            })
        };
        return await SendAndReadStringAsync(request, entityType, entityId, cancellationToken);
    }

    // --- MANAGE PRODUCTS (US2) ---
    public async Task<Merchandise> CreateProductAsync(CreateProductApiRequest requestDto, CancellationToken cancellationToken = default)
    {
        var json = await SendWithBodyAndReadStringAsync(HttpMethod.Post, PlatziStoreEndpoints.Products, requestDto, "Merchandise", null, cancellationToken);
        return _parser.ParseProduct(json);
    }

    public async Task<Merchandise> UpdateProductAsync(int id, UpdateProductApiRequest requestDto, CancellationToken cancellationToken = default)
    {
        var url = string.Format(PlatziStoreEndpoints.ProductById, id);
        var json = await SendWithBodyAndReadStringAsync(HttpMethod.Put, url, requestDto, "Merchandise", id, cancellationToken);
        return _parser.ParseProduct(json);
    }

    public async Task<bool> DeleteProductAsync(int id, CancellationToken cancellationToken = default)
    {
        var url = string.Format(PlatziStoreEndpoints.ProductById, id);
        var request = new HttpRequestMessage(HttpMethod.Delete, url);
        await SendAndReadStringAsync(request, "Merchandise", id, cancellationToken);
        return true; 
    }

    // --- CATEGORIES (US3) ---
    public async Task<IReadOnlyList<ProductGroup>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, PlatziStoreEndpoints.Categories);
        var json = await SendAndReadStringAsync(request, "ProductGroup", null, cancellationToken);
        return _parser.ParseCategories(json);
    }

    public async Task<ProductGroup> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var url = string.Format(PlatziStoreEndpoints.CategoryById, id);
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        var json = await SendAndReadStringAsync(request, "ProductGroup", id, cancellationToken);
        return _parser.ParseCategory(json);
    }

    public async Task<ProductGroup> GetCategoryBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var url = string.Format(PlatziStoreEndpoints.CategoryBySlug, slug);
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        var json = await SendAndReadStringAsync(request, "ProductGroup", slug, cancellationToken);
        return _parser.ParseCategory(json);
    }

    public async Task<IReadOnlyList<Merchandise>> GetProductsByCategoryAsync(int categoryId, int? offset = null, int? limit = null, CancellationToken cancellationToken = default)
    {
        var url = string.Format(PlatziStoreEndpoints.CategoryProducts, categoryId);
        if (offset.HasValue && limit.HasValue)
        {
            url += $"?offset={offset.Value}&limit={limit.Value}";
        }
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        var json = await SendAndReadStringAsync(request, "Merchandise", categoryId, cancellationToken);
        return _parser.ParseProducts(json);
    }

    public async Task<ProductGroup> CreateCategoryAsync(CreateCategoryApiRequest requestDto, CancellationToken cancellationToken = default)
    {
        var json = await SendWithBodyAndReadStringAsync(HttpMethod.Post, PlatziStoreEndpoints.Categories, requestDto, "ProductGroup", null, cancellationToken);
        return _parser.ParseCategory(json);
    }

    public async Task<ProductGroup> UpdateCategoryAsync(int id, UpdateCategoryApiRequest requestDto, CancellationToken cancellationToken = default)
    {
        var url = string.Format(PlatziStoreEndpoints.CategoryById, id);
        var json = await SendWithBodyAndReadStringAsync(HttpMethod.Put, url, requestDto, "ProductGroup", id, cancellationToken);
        return _parser.ParseCategory(json);
    }

    public async Task<bool> DeleteCategoryAsync(int id, CancellationToken cancellationToken = default)
    {
        var url = string.Format(PlatziStoreEndpoints.CategoryById, id);
        var request = new HttpRequestMessage(HttpMethod.Delete, url);
        await SendAndReadStringAsync(request, "ProductGroup", id, cancellationToken);
        return true;
    }

    // --- USERS (US4) ---
    public async Task<IReadOnlyList<StoreCustomer>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, PlatziStoreEndpoints.Users);
        var json = await SendAndReadStringAsync(request, "StoreCustomer", null, cancellationToken);
        return _parser.ParseUsers(json);
    }

    public async Task<StoreCustomer> GetUserByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var url = string.Format(PlatziStoreEndpoints.UserById, id);
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        var json = await SendAndReadStringAsync(request, "StoreCustomer", id, cancellationToken);
        return _parser.ParseUser(json);
    }

    public async Task<StoreCustomer> CreateUserAsync(CreateUserApiRequest requestDto, CancellationToken cancellationToken = default)
    {
        var json = await SendWithBodyAndReadStringAsync(HttpMethod.Post, PlatziStoreEndpoints.Users, requestDto, "StoreCustomer", null, cancellationToken);
        return _parser.ParseUser(json);
    }

    public async Task<StoreCustomer> UpdateUserAsync(int id, UpdateUserApiRequest requestDto, CancellationToken cancellationToken = default)
    {
        var url = string.Format(PlatziStoreEndpoints.UserById, id);
        var json = await SendWithBodyAndReadStringAsync(HttpMethod.Put, url, requestDto, "StoreCustomer", id, cancellationToken);
        return _parser.ParseUser(json);
    }

    public async Task<bool> CheckEmailAvailabilityAsync(string email, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, PlatziStoreEndpoints.UserEmailAvailability)
        {
            Content = JsonContent.Create(new { email })
        };
        var json = await SendAndReadStringAsync(request, "EmailAvailability", email, cancellationToken);
        var dto = JsonSerializer.Deserialize<EmailAvailabilityApiDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return dto?.IsAvailable ?? false;
    }

    // --- AUTH (US5) ---
    public async Task<AuthTokensApiDto> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var request = new LoginApiRequest { Email = email, Password = password };
        var json = await SendWithBodyAndReadStringAsync(HttpMethod.Post, PlatziStoreEndpoints.AuthLogin, request, "AuthTokens", null, cancellationToken);
        return JsonSerializer.Deserialize<AuthTokensApiDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) 
            ?? throw new ExternalServiceException("PlatziStoreApi", "Failed to parse auth tokens.");
    }

    public async Task<StoreCustomer> GetProfileAsync(string accessToken, CancellationToken cancellationToken = default)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, PlatziStoreEndpoints.AuthProfile);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        var json = await SendAndReadStringAsync(request, "StoreCustomer", "profile", cancellationToken);
        return _parser.ParseUser(json);
    }

    public async Task<AuthTokensApiDto> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var request = new RefreshTokenApiRequest { RefreshToken = refreshToken };
        var json = await SendWithBodyAndReadStringAsync(HttpMethod.Post, PlatziStoreEndpoints.AuthRefreshToken, request, "AuthTokens", null, cancellationToken);
        return JsonSerializer.Deserialize<AuthTokensApiDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new ExternalServiceException("PlatziStoreApi", "Failed to parse refreshed auth tokens.");
    }
}
