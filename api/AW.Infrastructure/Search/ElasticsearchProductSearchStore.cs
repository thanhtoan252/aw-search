using AW.Application.Interfaces;
using AW.Domain.Common;
using AW.Domain.Entities;
using AW.Domain.Models;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Aggregations;
using Elastic.Clients.Elasticsearch.Core.Explain;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.Fluent;
using Microsoft.Extensions.Logging;
using IndexStats = AW.Domain.Models.IndexStats;

namespace AW.Infrastructure.Search;

internal sealed class ElasticsearchProductSearchStore(
    ElasticsearchClient client,
    ElasticsearchQueryHelper queryHelper,
    ILogger<ElasticsearchProductSearchStore> logger) : IProductSearchStore
{
    internal const string IndexName = "aw-products";

    // ── Index Management ──────────────────────────────────────────────────────

    public async Task EnsureIndexExistsAsync(CancellationToken ct = default)
    {
        var exists = await client.Indices.ExistsAsync(IndexName, ct);
        if (exists.Exists) { return; }

        await CreateIndexAsync(ct);
    }

    private async Task CreateIndexAsync(CancellationToken ct)
    {
        logger.LogInformation("Creating Elasticsearch index '{Index}'", IndexName);

        var response = await client.Indices.CreateAsync(IndexName, cfg => cfg
            .Settings(s => s
                .NumberOfShards(1)
                .NumberOfReplicas(0)
                .Analysis(a => a
                    .Analyzers(an => an
                        .Custom("product_analyzer", ca => ca
                            .Tokenizer("standard")
                            .Filter(["lowercase", "asciifolding"])
                        )
                    )
                )
            )
            .Mappings(m => m
                .Properties<ProductDocument>(p => p
                    .IntegerNumber(f => f.ProductId)
                    .Text(f => f.Name, t => t
                        .Analyzer("product_analyzer")
                        .Fields(ff => ff.Keyword("keyword"))
                    )
                    .Keyword(f => f.ProductNumber)
                    .Keyword(f => f.Color!)
                    .FloatNumber(f => f.ListPrice)
                    .FloatNumber(f => f.StandardCost)
                    .Keyword(f => f.Size!)
                    .Keyword(f => f.ProductLine!)
                    .Keyword(f => f.Class!)
                    .Keyword(f => f.CategoryName!)
                    .Keyword(f => f.SubcategoryName!)
                    .Text(f => f.ModelName!, t => t.Analyzer("product_analyzer"))
                    .Text(f => f.Description!, t => t.Analyzer("product_analyzer"))
                    .Boolean(f => f.IsDiscontinued)
                    .Date(f => f.SellStartDate)
                    .Date(f => f.IndexedAt)
                )
            ), ct);

        if (!response.IsSuccess())
        {
            logger.LogError("Failed to create index: {Error}", response.ElasticsearchServerError?.Error?.Reason);
        }
    }

    // ── Indexing ──────────────────────────────────────────────────────────────

    public async Task BulkIndexAsync(IReadOnlyList<Product> products, CancellationToken ct = default)
    {
        var documents = products.Select(ProductDocumentMapper.ToDocument).ToList();

        var response = await client.BulkAsync(b => b
            .Index(IndexName)
            .IndexMany(documents, (op, doc) => op.Id(doc.ProductId.ToString())), ct);

        if (response.Errors)
        {
            logger.LogWarning("Bulk index had {Errors} errors in batch of {Total}",
                response.ItemsWithErrors.Count(), documents.Count);
        }
    }

    // ── Search ────────────────────────────────────────────────────────────────

    public async Task<Result<SearchResponse>> SearchAsync(SearchFilter filter, CancellationToken ct = default)
    {
        var response = await ExecuteSearchAsync(filter, ct);

        if (!response.IsSuccess())
        {
            return SearchFailure(response);
        }

        return Result<SearchResponse>.Success(ToSearchResponse(response, filter));
    }

    private Task<Elastic.Clients.Elasticsearch.SearchResponse<ProductDocument>> ExecuteSearchAsync(
        SearchFilter filter,
        CancellationToken ct) =>
        client.SearchAsync<ProductDocument>(s => s
            .Indices(IndexName)
            .From((filter.Page - 1) * filter.PageSize)
            .Size(filter.PageSize)
            .Explain(filter.HasTextQuery)
            .Query(q => queryHelper.BuildQuery(q, filter))
            .Aggregations(BuildFacetAggregations)
            .Sort(so => so
                .Score(sc => sc.Order(SortOrder.Desc))
                .Field(f => f.ListPrice, fd => fd.Order(SortOrder.Asc))
            ), ct);

    private static void BuildFacetAggregations(FluentDictionaryOfStringAggregation<ProductDocument> a)
    {
        a.Add("categories", agg => agg.Terms(t => t.Field(f => f.CategoryName).Size(20)))
            .Add("colors", agg => agg.Terms(t => t.Field(f => f.Color).Size(20)))
            .Add("product_lines", agg => agg.Terms(t => t.Field(f => f.ProductLine).Size(10)));
    }

    private Result<SearchResponse> SearchFailure(Elastic.Clients.Elasticsearch.SearchResponse<ProductDocument> response)
    {
        var reason = response.ElasticsearchServerError?.Error?.Reason;
        logger.LogError("Elasticsearch search failed: {Error}", reason);

        return Result<SearchResponse>.Failure(ElasticsearchUnavailable(reason ?? "Product search failed."));
    }

    private static SearchResponse ToSearchResponse(
        Elastic.Clients.Elasticsearch.SearchResponse<ProductDocument> response,
        SearchFilter filter)
    {
        var hits = response.Hits
            .Where(h => h.Source is not null)
            .ToList();
        var maxScore = hits.Count == 0 ? 0 : hits.Max(h => h.Score ?? 0);

        return new SearchResponse
        {
            Items = [..hits.Select(hit => ProductDocumentMapper.ToResult(
                hit.Source!,
                hit.Score,
                CalculateMatchRatio(hit.Score, maxScore),
                ToSearchExplain(hit.Explanation)))],
            Total = response.Total,
            Page = filter.Page,
            PageSize = filter.PageSize,
            Facets = ElasticsearchQueryHelper.ExtractFacets(response.Aggregations),
        };
    }

    private static int CalculateMatchRatio(double? score, double maxScore)
    {
        if (maxScore <= 0)
        {
            return 100;
        }

        return (int)Math.Round((score ?? 0) / maxScore * 100);
    }

    private static SearchExplain? ToSearchExplain(Explanation? explanation)
    {
        if (explanation is null)
        {
            return null;
        }

        return new SearchExplain
        {
            Value = explanation.Value,
            Description = explanation.Description,
            Details = ToSearchExplainDetails(explanation.Details),
        };
    }

    private static IReadOnlyList<SearchExplain> ToSearchExplainDetails(
        IReadOnlyCollection<ExplanationDetail>? details) =>
        details is null
            ? []
            : FlattenExplanationDetails(details)
                .Where(d => IsUsefulExplainDetail(d.Description))
                .OrderByDescending(d => d.Value)
                .Take(5)
                .Select(d => new SearchExplain
                {
                    Value = d.Value,
                    Description = d.Description,
                    Details = [],
                })
                .ToList();

    private static IEnumerable<ExplanationDetail> FlattenExplanationDetails(IEnumerable<ExplanationDetail> details)
    {
        foreach (var detail in details)
        {
            yield return detail;

            foreach (var child in FlattenExplanationDetails(detail.Details ?? []))
            {
                yield return child;
            }
        }
    }

    private static bool IsUsefulExplainDetail(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return false;
        }

        return description.Contains("weight(", StringComparison.OrdinalIgnoreCase)
            || description.Contains("fieldWeight", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<Result<ProductSearchResult>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var response = await ExecuteGetByIdAsync(id, ct);

        if (response.ElasticsearchServerError is not null)
        {
            return ProductLookupFailure(response);
        }

        if (!response.Found || response.Source is null)
        {
            return ProductNotFound(id);
        }

        if (!response.IsSuccess())
        {
            return ProductLookupFailure(response);
        }

        return ProductDocumentMapper.ToResult(response.Source);
    }

    private Task<GetResponse<ProductDocument>> ExecuteGetByIdAsync(int id, CancellationToken ct) =>
        client.GetAsync<ProductDocument>(id.ToString(), idx => idx
            .Index(IndexName), ct);

    private Result<ProductSearchResult> ProductLookupFailure(GetResponse<ProductDocument> response)
    {
        var reason = response.ElasticsearchServerError?.Error?.Reason;
        logger.LogError("Elasticsearch get product failed: {Error}", reason ?? response.ApiCallDetails.HttpStatusCode?.ToString());

        return Result<ProductSearchResult>.Failure(ElasticsearchUnavailable(reason ?? "Product lookup failed."));
    }

    private static Result<ProductSearchResult> ProductNotFound(int id) =>
        Result<ProductSearchResult>.Failure(Error.NotFound(
            "Products.NotFound",
            $"Product {id} was not found in the search index."));

    // ── Stats ─────────────────────────────────────────────────────────────────

    public async Task<Result<IndexStats>> GetStatsAsync(CancellationToken ct = default)
    {
        var countResponse = await client.CountAsync(c => c.Indices(IndexName), ct);
        var statsResponse = await client.Indices.StatsAsync(s => s.Indices(IndexName), ct);

        if (!countResponse.IsSuccess() || !statsResponse.IsSuccess())
        {
            var reason = countResponse.ElasticsearchServerError?.Error?.Reason
                ?? statsResponse.ElasticsearchServerError?.Error?.Reason;

            logger.LogError("Elasticsearch index stats failed: {Error}", reason);
            return Result<IndexStats>.Failure(ElasticsearchUnavailable(reason ?? "Index status lookup failed."));
        }

        return new IndexStats(
            IndexName,
            countResponse.Count,
            statsResponse.Indices?[IndexName]?.Total?.Store?.SizeInBytes,
            true);
    }

    private static Error ElasticsearchUnavailable(string description) =>
        Error.ExternalDependency("Search.ElasticsearchUnavailable", description);
}
