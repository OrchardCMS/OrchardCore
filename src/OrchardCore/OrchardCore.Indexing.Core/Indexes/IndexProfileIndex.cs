using YesSql.Indexes;

namespace OrchardCore.Indexing.Core.Indexes;

public sealed class IndexProfileIndex : MapIndex
{
    public string IndexProfileId { get; set; }

    public string Name { get; set; }

    public string IndexName { get; set; }

    public string ProviderName { get; set; }

    public string Type { get; set; }
}
