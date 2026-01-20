using YesSql.Indexes;

namespace OrchardCore.Taxonomies.Indexing;

public sealed class TaxonomyIndex : MapIndex
{
    public string TaxonomyContentItemId { get; set; }

    public string ContentItemId { get; set; }

    public string ContentType { get; set; }

    public string ContentPart { get; set; }

    public string ContentField { get; set; }

    public string TermContentItemId { get; set; }

    public bool Published { get; set; }

    public bool Latest { get; set; }
}
