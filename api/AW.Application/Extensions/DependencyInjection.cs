using AW.Application.Interfaces;
using AW.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AW.Application.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IProductSearchService, ProductSearchService>();
        services.AddScoped<IIndexingService, IndexingService>();

        return services;
    }
}
