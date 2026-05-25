namespace AW.Infrastructure.Search;

public sealed class ElasticsearchOptions
{
    public const string SectionName = "Elasticsearch";

    public required string Uri { get; set; }

    public bool EnableDebugMode { get; init; }
}
