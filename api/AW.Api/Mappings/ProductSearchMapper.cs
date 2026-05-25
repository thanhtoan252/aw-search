using AW.Api.DTOs.Requests;
using AW.Api.DTOs.Responses;
using AW.Domain.Models;

namespace AW.Api.Mappings;

internal static class ProductSearchMapper
{
    internal static SearchFilter ToFilter(this ProductSearchRequestDto req) => new()
    {
        Query = req.Q,
        Category = req.Category,
        Color = req.Color,
        ProductLine = req.ProductLine,
        MinPrice = req.MinPrice,
        MaxPrice = req.MaxPrice,
        Page = Math.Max(1, req.Page),
        PageSize = Math.Clamp(req.PageSize, 1, 100),
    };

    internal static SearchResultDto ToDto(this SearchResponse response) => new()
    {
        Items = response.Items.Select(ToDto).ToList(),
        Total = response.Total,
        Page = response.Page,
        PageSize = response.PageSize,
        TotalPages = (int)Math.Ceiling(response.Total / (double)response.PageSize),
        Facets = response.Facets.ToDictionary(
            kvp => kvp.Key,
            kvp => ToFacetDtoList(kvp.Value)),
    };

    private static IReadOnlyList<FacetDto> ToFacetDtoList(IReadOnlyList<FacetItem> facets) =>
        facets
            .Select(f => new FacetDto(f.Value, f.Count))
            .ToList();

    internal static ProductDto ToDto(this ProductSearchResult r) => new()
    {
        ProductId = r.ProductId,
        Name = r.Name,
        ProductNumber = r.ProductNumber,
        Color = r.Color,
        ListPrice = r.ListPrice,
        Size = r.Size,
        CategoryName = r.CategoryName,
        SubcategoryName = r.SubcategoryName,
        ModelName = r.ModelName,
        Description = r.Description,
        ProductLine = r.ProductLine,
        IsDiscontinued = r.IsDiscontinued,
    };

    internal static IndexStatusDto ToDto(this IndexStats stats) => new(
        stats.IndexName,
        stats.DocumentCount,
        stats.SizeBytes,
        stats.IsHealthy
    );
}
