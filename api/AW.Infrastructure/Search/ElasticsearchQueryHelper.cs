using System.Linq.Expressions;
using AW.Domain.Models;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Aggregations;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AW.Infrastructure.Search;

public sealed class ElasticsearchQueryHelper(IOptions<SearchConfiguration> options, ILogger<ElasticsearchQueryHelper> logger)
{
    private readonly SearchConfiguration _config = options.Value;

    internal void BuildQuery(QueryDescriptor<ProductDocument> q, SearchFilter filter)
    {
        var filters = BuildFilters(filter);
        
        if (filter.HasTextQuery)
        {
            BuildTextQuery(q, filter, filters);
            return;
        }

        if (filters.Count > 0)
        {
            BuildFilteredQuery(q, filters);
            return;
        }

        BuildMatchAllQuery(q);
    }

    private static List<Action<QueryDescriptor<ProductDocument>>> BuildFilters(SearchFilter filter)
    {
        var filters = new List<Action<QueryDescriptor<ProductDocument>>>();

        AddTermFilter(filters, filter.Category, p => p.CategoryName);
        AddTermFilter(filters, filter.Color, p => p.Color);
        AddTermFilter(filters, filter.ProductLine, p => p.ProductLine);
        AddPriceRangeFilter(filters, filter);

        return filters;
    }

    private static void AddTermFilter(
        ICollection<Action<QueryDescriptor<ProductDocument>>> filters,
        string? value,
        Expression<Func<ProductDocument, object?>> field)
    {
        if (string.IsNullOrWhiteSpace(value)) { return; }

        filters.Add(f => f.Term(t => t.Field(field).Value(value)));
    }

    private static void AddPriceRangeFilter(
        ICollection<Action<QueryDescriptor<ProductDocument>>> filters,
        SearchFilter filter)
    {
        if (!filter.HasPriceRange) { return; }

        filters.Add(f => f.Range(r => r.Number(nr =>
        {
            nr.Field(p => p.ListPrice);
            if (filter.MinPrice.HasValue) nr.Gte((double)filter.MinPrice.Value);
            if (filter.MaxPrice.HasValue) nr.Lte((double)filter.MaxPrice.Value);
        })));
    }

    private void BuildTextQuery(
        QueryDescriptor<ProductDocument> q,
        SearchFilter filter,
        IReadOnlyCollection<Action<QueryDescriptor<ProductDocument>>> filters)
    {
        var searchFields = BuildSearchFields();
        if (searchFields.Count == 0)
        {
            logger.LogWarning("No search fields enabled in configuration");
            BuildMatchAllQuery(q);
            return;
        }

        q.Bool(b => b
            .Must(m => m.MultiMatch(mm => mm
                .Query(filter.Query!)
                .Fields(searchFields.ToArray())
                .Type(TextQueryType.BestFields)
                .Fuzziness(new Fuzziness("AUTO"))
            ))
            .Filter(filters.ToArray())
        );
    }

    private static void BuildFilteredQuery(
        QueryDescriptor<ProductDocument> q,
        IReadOnlyCollection<Action<QueryDescriptor<ProductDocument>>> filters) =>
        q.Bool(b => b.Filter(filters.ToArray()));

    private static void BuildMatchAllQuery(QueryDescriptor<ProductDocument> q) =>
        q.MatchAll(m => { });

    private List<string> BuildSearchFields()
    {
        var fields = new List<string>();

        foreach (var field in _config.Fields)
        {
            if (!field.Enabled)
            {
                logger.LogDebug("Search field '{Field}' is disabled", field.Name);
                continue;
            }

            var fieldSpec = field.Boost == 1.0m 
                ? field.Name 
                : $"{field.Name}^{field.Boost}";
            
            fields.Add(fieldSpec);
            logger.LogDebug("Added search field '{Field}' with boost {Boost}, fuzziness {Fuzziness}", 
                field.Name, field.Boost, field.Fuzziness);
        }

        return fields;
    }

    internal static IReadOnlyDictionary<string, IReadOnlyList<FacetItem>> ExtractFacets(AggregateDictionary? aggregations)
    {
        var facets = new Dictionary<string, IReadOnlyList<FacetItem>>();

        if (aggregations is null) return facets;

        void TryExtract(string aggKey, string facetKey)
        {
            if (aggregations.TryGetValue(aggKey, out var agg) && agg is StringTermsAggregate terms)
            {
                facets[facetKey] = terms.Buckets
                    .Where(b => !string.IsNullOrWhiteSpace(b.Key.Value?.ToString()))
                    .Select(b => new FacetItem(b.Key.Value?.ToString() ?? "", b.DocCount))
                    .ToList();
            }
        }

        TryExtract("categories", "categories");
        TryExtract("colors", "colors");
        TryExtract("product_lines", "productLines");

        return facets;
    }
}
