namespace AW.Api.Endpoints.Products.V1.DTOs.Responses;

public sealed record SearchExplainDto
{
    public double Value { get; init; }
    public string Description { get; init; } = string.Empty;
    public IReadOnlyList<SearchExplainDto> Details { get; init; } = [];
}
