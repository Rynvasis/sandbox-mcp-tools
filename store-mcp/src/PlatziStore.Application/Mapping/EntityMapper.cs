using PlatziStore.Domain.Entities;
using PlatziStore.Application.DataTransfer;

namespace PlatziStore.Application.Mapping;

public static class EntityMapper
{
    public static CatalogItemSummary ToSummary(Merchandise entity)
    {
        return new CatalogItemSummary
        {
            Id = entity.Id,
            Title = entity.Title,
            Slug = entity.Slug.Value,
            Price = entity.Price.Value,
            CategoryName = entity.Category.Name
        };
    }

    public static CatalogItemDetail ToDetail(Merchandise entity)
    {
        return new CatalogItemDetail
        {
            Id = entity.Id,
            Title = entity.Title,
            Slug = entity.Slug.Value,
            Price = entity.Price.Value,
            Description = entity.Description,
            CategoryId = entity.Category.Id,
            CategoryName = entity.Category.Name,
            CategorySlug = entity.Category.Slug.Value,
            ImageUrls = entity.Images.Select(img => img.Value).ToList()
        };
    }

    public static CategorySummary ToSummary(ProductGroup entity)
    {
        return new CategorySummary
        {
            Id = entity.Id,
            Name = entity.Name,
            Slug = entity.Slug.Value,
            ImageUrl = entity.CoverImage.Value
        };
    }

    public static CustomerProfile ToProfile(StoreCustomer entity)
    {
        return new CustomerProfile
        {
            Id = entity.Id,
            Email = entity.Email.Value,
            DisplayName = entity.DisplayName,
            Role = entity.Role,
            AvatarUrl = entity.Avatar.Value
        };
    }

    public static AuthTokenPair ToTokenPair(string accessToken, string refreshToken)
    {
        return new AuthTokenPair
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }
}
