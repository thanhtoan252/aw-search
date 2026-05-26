namespace AW.Application.Options;

public sealed class IndexingOptions
{
    public const string SectionName = "Indexing";

    public int BatchSize { get; init; } = 500;
}
