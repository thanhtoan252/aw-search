using AW.Application.Interfaces;
using AW.Infrastructure.BackgroundJobs;
using AW.Infrastructure.Persistence;
using AW.Infrastructure.Persistence.Repositories;
using AW.Infrastructure.Search;
using Elastic.Clients.Elasticsearch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace AW.Infrastructure.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddDatabase(configuration)
            .AddElasticsearch(configuration)
            .AddSearchConfiguration()
            .AddRepositories()
            .AddBackgroundJobs();

        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AdventureWorks")
            ?? throw new InvalidOperationException("Connection string 'AdventureWorks' is not configured");

        services.AddDbContext<AdventureWorksDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
                sqlOptions.EnableRetryOnFailure(3, TimeSpan.FromSeconds(2), null)));

        return services;
    }

    private static IServiceCollection AddElasticsearch(this IServiceCollection services, IConfiguration configuration)
    {
        var elasticsearchSection = configuration.GetSection(ElasticsearchOptions.SectionName);

        services.Configure<ElasticsearchOptions>(elasticsearchSection.Bind);
        services.AddSingleton(sp =>
        {
            var esOptions = sp.GetRequiredService<IOptions<ElasticsearchOptions>>().Value;
            var settings = CreateElasticsearchSettings(esOptions);

            return new ElasticsearchClient(settings);
        });

        return services;
    }

    private static ElasticsearchClientSettings CreateElasticsearchSettings(ElasticsearchOptions esOptions)
    {
        if (string.IsNullOrWhiteSpace(esOptions.Uri))
        {
            throw new InvalidOperationException("Elasticsearch configuration is missing");
        }

        var settings = new ElasticsearchClientSettings(new Uri(esOptions.Uri));
        return esOptions.EnableDebugMode ? EnableDebugLogging(settings) : settings;
    }

    private static ElasticsearchClientSettings EnableDebugLogging(ElasticsearchClientSettings settings) =>
        settings.OnRequestCompleted(details =>
        {
            Console.WriteLine($"ES Request: {details.HttpMethod} {details.Uri}");
            WriteBody("Body", details.RequestBodyInBytes);
            WriteBody("Response", details.ResponseBodyInBytes);
        });

    private static void WriteBody(string label, byte[]? body)
    {
        if (body is null)
        {
            return;
        }

        Console.WriteLine($"{label}: {System.Text.Encoding.UTF8.GetString(body)}");
    }

    private static IServiceCollection AddSearchConfiguration(this IServiceCollection services)
    {
        var searchConfig = BuildDefaultSearchConfiguration();

        services.Configure<SearchConfiguration>(opts =>
        {
            opts.Fields = searchConfig.Fields;
        });
        services.AddScoped<ElasticsearchQueryHelper>();

        return services;
    }

    private static SearchConfiguration BuildDefaultSearchConfiguration() =>
        new SearchConfigurationBuilder()
            .AddField("name", boost: 3.0m, fuzziness: "AUTO")
            .AddField("productNumber", boost: 2.0m, fuzziness: "AUTO")
            .AddField("modelName", boost: 2.0m, fuzziness: "AUTO")
            .AddField("description", boost: 1.0m, fuzziness: "AUTO")
            .AddField("categoryName", boost: 1.0m, fuzziness: "AUTO")
            .AddField("subcategoryName", boost: 1.0m, fuzziness: "AUTO")
            .Build();

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IProductSearchStore, ElasticsearchProductSearchStore>();

        return services;
    }

    private static IServiceCollection AddBackgroundJobs(this IServiceCollection services)
    {
        services.AddSingleton<ProductIndexingBackgroundJob>();
        services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<ProductIndexingBackgroundJob>());
        services.AddSingleton<IIndexingTrigger>(sp => sp.GetRequiredService<ProductIndexingBackgroundJob>());

        return services;
    }
}
