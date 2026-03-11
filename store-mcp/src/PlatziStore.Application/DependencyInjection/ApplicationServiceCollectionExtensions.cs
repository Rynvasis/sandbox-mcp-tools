using Microsoft.Extensions.DependencyInjection;
using PlatziStore.Application.Contracts;
using PlatziStore.Application.Services;

namespace PlatziStore.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddPlatziStoreApplication(this IServiceCollection services)
    {
        services.AddScoped<ICatalogQueryService, CatalogQueryHandler>();
        services.AddScoped<ICatalogCommandService, CatalogCommandHandler>();
        services.AddScoped<ICategoryQueryService, CategoryQueryHandler>();
        services.AddScoped<ICategoryCommandService, CategoryCommandHandler>();
        services.AddScoped<ICustomerAccountService, CustomerAccountHandler>();
        services.AddScoped<IIdentityAccessService, IdentityAccessHandler>();

        return services;
    }
}
