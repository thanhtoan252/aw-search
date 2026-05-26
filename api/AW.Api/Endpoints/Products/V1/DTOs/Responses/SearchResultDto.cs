namespace AW.Api.Endpoints.Products.V1.DTOs.Responses;

public sealed record SearchResultDto
{
    public IReadOnlyList<ProductDto> Items { get; init; } = [];
    public long Total { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
    public IReadOnlyDictionary<string, IReadOnlyList<FacetDto>> Facets { get; init; }
        = new Dictionary<string, IReadOnlyList<FacetDto>>();
}
