namespace AW.Infrastructure.Search;

/// <summary>
/// Internal document model stored in Elasticsearch.
/// Not exposed outside Infrastructure layer.
/// </summary>
internal sealed record ProductDocument
{
    public int ProductId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ProductNumber { get; init; } = string.Empty;
    public string? Color { get; init; }
    public decimal ListPrice { get; init; }
    public decimal StandardCost { get; init; }
    public string? Size { get; init; }
    public decimal? Weight { get; init; }
    public string? ProductLine { get; init; }
    public string? Class { get; init; }
    public string? CategoryName { get; init; }
    public string? SubcategoryName { get; init; }
    public string? ModelName { get; init; }
    public string? Description { get; init; }
    public bool IsDiscontinued { get; init; }
    public DateTime SellStartDate { get; init; }
    public DateTime IndexedAt { get; init; }
}
