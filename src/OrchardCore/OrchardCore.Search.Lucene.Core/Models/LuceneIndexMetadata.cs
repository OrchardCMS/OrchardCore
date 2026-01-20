namespace OrchardCore.Search.Lucene.Models;

public sealed class LuceneIndexMetadata
{
    public bool StoreSourceData { get; set; }

    public string AnalyzerName { get; set; }

    public LuceneIndexMap IndexMappings { get; set; }
}
