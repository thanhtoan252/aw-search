namespace AW.Domain.Models;

public sealed record SearchResponse
{
    public IReadOnlyList<ProductSearchResult> Items { get; init; } = [];
    public long Total { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public IReadOnlyDictionary<string, IReadOnlyList<FacetItem>> Facets { get; init; }
        = new Dictionary<string, IReadOnlyList<FacetItem>>();
}
