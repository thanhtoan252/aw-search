using AW.Application.Interfaces;
using AW.Application.Options;
using AW.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AW.Application.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<IndexingOptions>(configuration.GetSection(IndexingOptions.SectionName).Bind);
        services.AddScoped<IProductSearchService, ProductSearchService>();
        services.AddScoped<IIndexingService, IndexingService>();

        return services;
    }
}
