namespace AW.Api.Endpoints.Products.V1.DTOs.Responses;

public sealed record ProductDto
{
    public int ProductId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ProductNumber { get; init; } = string.Empty;
    public string? Color { get; init; }
    public decimal ListPrice { get; init; }
    public string? Size { get; init; }
    public string? CategoryName { get; init; }
    public string? SubcategoryName { get; init; }
    public string? ModelName { get; init; }
    public string? Description { get; init; }
    public string? ProductLine { get; init; }
    public bool IsDiscontinued { get; init; }
    public double? SearchScore { get; init; }
    public int MatchRatio { get; init; }
    public SearchExplainDto? Explain { get; init; }
}
