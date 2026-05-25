namespace AW.Infrastructure.Search;

public sealed class SearchConfigurationBuilder
{
    private readonly List<SearchFieldConfig> _fields = [];

    public SearchConfigurationBuilder AddField(
        string name,
        decimal boost = 1.0m,
        string fuzziness = "AUTO",
        bool enabled = true)
    {
        _fields.Add(new SearchFieldConfig
        {
            Name = name,
            Boost = boost,
            Fuzziness = fuzziness,
            Enabled = enabled
        });
        
        return this;
    }

    public SearchConfiguration Build()
    {
        if (_fields.Count == 0)
        {
            throw new InvalidOperationException("At least one search field must be configured");
        }

        return new SearchConfiguration { Fields = [.._fields] };
    }
}
