using YesSql.Indexes;

namespace OrchardCore.Taxonomies.Indexing;

public sealed class SortedTaxonomyIndex : MapIndex
{
    public string ContentItemId { get; set; }

    public int Order { get; set; }
}
