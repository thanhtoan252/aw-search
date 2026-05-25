namespace AW.Domain.Models;

/// <summary>
/// Value object encapsulating product search criteria.
/// </summary>
public sealed record SearchFilter
{
    public string? Query { get; init; }
    public string? Category { get; init; }
    public string? Color { get; init; }
    public string? ProductLine { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;

    public bool HasTextQuery => !string.IsNullOrWhiteSpace(Query);
    public bool HasPriceRange => MinPrice.HasValue || MaxPrice.HasValue;
}
