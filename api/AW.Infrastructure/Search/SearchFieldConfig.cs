namespace AW.Infrastructure.Search;

public sealed class SearchFieldConfig
{
    public required string Name { get; set; }
    public decimal Boost { get; init; } = 1.0m;
    public string? Fuzziness { get; init; } = "AUTO";
    public bool Enabled { get; init; } = true;
}
