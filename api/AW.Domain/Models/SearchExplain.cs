namespace AW.Domain.Models;

public sealed record SearchExplain
{
    public double Value { get; init; }
    public string Description { get; init; } = string.Empty;
    public IReadOnlyList<SearchExplain> Details { get; init; } = [];
}
