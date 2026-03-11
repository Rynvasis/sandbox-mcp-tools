using System.Text.Json;
using PlatziStore.Domain.Entities;
using PlatziStore.Domain.ValueObjects;
using PlatziStore.Infrastructure.ApiClients.Dtos;

namespace PlatziStore.Infrastructure.ApiClients;

public class PlatziStoreResponseParser
{
    private readonly JsonSerializerOptions _jsonOptions;

    public PlatziStoreResponseParser()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    public Merchandise ParseProduct(string json)
    {
        var dto = JsonSerializer.Deserialize<ProductApiDto>(json, _jsonOptions) 
            ?? throw new JsonException("Failed to deserialize product JSON.");
        return MapProduct(dto);
    }

    public IReadOnlyList<Merchandise> ParseProducts(string json)
    {
        var dtos = JsonSerializer.Deserialize<List<ProductApiDto>>(json, _jsonOptions) 
            ?? throw new JsonException("Failed to deserialize products JSON.");
        return dtos.Select(MapProduct).ToList().AsReadOnly();
    }

    public ProductGroup ParseCategory(string json)
    {
        var dto = JsonSerializer.Deserialize<CategoryApiDto>(json, _jsonOptions) 
            ?? throw new JsonException("Failed to deserialize category JSON.");
        return MapCategory(dto);
    }

    public IReadOnlyList<ProductGroup> ParseCategories(string json)
    {
        var dtos = JsonSerializer.Deserialize<List<CategoryApiDto>>(json, _jsonOptions) 
            ?? throw new JsonException("Failed to deserialize categories JSON.");
        return dtos.Select(MapCategory).ToList().AsReadOnly();
    }

    public StoreCustomer ParseUser(string json)
    {
        var dto = JsonSerializer.Deserialize<UserApiDto>(json, _jsonOptions) 
            ?? throw new JsonException("Failed to deserialize user JSON.");
        return MapUser(dto);
    }

    public IReadOnlyList<StoreCustomer> ParseUsers(string json)
    {
        var dtos = JsonSerializer.Deserialize<List<UserApiDto>>(json, _jsonOptions) 
            ?? throw new JsonException("Failed to deserialize users JSON.");
        return dtos.Select(MapUser).ToList().AsReadOnly();
    }

    private static Merchandise MapProduct(ProductApiDto dto)
    {
        return new Merchandise
        {
            Id = dto.Id,
            Title = dto.Title,
            Slug = SlugIdentifier.From(dto.Slug),
            Price = MonetaryAmount.From(dto.Price),
            Description = dto.Description,
            Category = MapCategory(dto.Category),
            Images = dto.Images?.Select(ImageUrl.From).ToList().AsReadOnly() ?? new List<ImageUrl>().AsReadOnly()
        };
    }

    private static ProductGroup MapCategory(CategoryApiDto dto)
    {
        return new ProductGroup
        {
            Id = dto.Id,
            Name = dto.Name,
            Slug = string.IsNullOrWhiteSpace(dto.Slug) ? SlugIdentifier.From("default-slug") : SlugIdentifier.From(dto.Slug),
            CoverImage = string.IsNullOrWhiteSpace(dto.Image) ? ImageUrl.From("https://example.com/default.jpg") : ImageUrl.From(dto.Image)
        };
    }

    private static StoreCustomer MapUser(UserApiDto dto)
    {
        return new StoreCustomer
        {
            Id = dto.Id,
            Email = EmailAddress.From(dto.Email),
            DisplayName = dto.Name ?? string.Empty,
            Role = dto.Role ?? string.Empty,
            Avatar = string.IsNullOrWhiteSpace(dto.Avatar) ? ImageUrl.From("https://example.com/avatar.jpg") : ImageUrl.From(dto.Avatar)
        };
    }
}
