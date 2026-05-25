namespace AW.Api.DTOs.Requests;

public sealed record ProductSearchRequestDto
{
    public string? Q { get; init; }
    public string? Category { get; init; }
    public string? Color { get; init; }
    public string? ProductLine { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
