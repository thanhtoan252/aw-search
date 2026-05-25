namespace AW.Infrastructure.Search;

public sealed class SearchConfiguration
{
    public const string SectionName = "Search";
    public required SearchFieldConfig[] Fields { get; set; }
}
