namespace PlatziStore.Infrastructure.ApiClients;

public static class PlatziStoreEndpoints
{
    public const string Products = "/api/v1/products";
    public const string ProductById = "/api/v1/products/{0}";
    public const string ProductBySlug = "/api/v1/products/slug/{0}";
    public const string ProductRelated = "/api/v1/products/{0}/related";
    public const string ProductRelatedBySlug = "/api/v1/products/slug/{0}/related";

    public const string Categories = "/api/v1/categories";
    public const string CategoryById = "/api/v1/categories/{0}";
    public const string CategoryBySlug = "/api/v1/categories/slug/{0}";
    public const string CategoryProducts = "/api/v1/categories/{0}/products";

    public const string Users = "/api/v1/users";
    public const string UserById = "/api/v1/users/{0}";
    public const string UserEmailAvailability = "/api/v1/users/is-available";

    public const string AuthLogin = "/api/v1/auth/login";
    public const string AuthProfile = "/api/v1/auth/profile";
    public const string AuthRefreshToken = "/api/v1/auth/refresh-token";
}
