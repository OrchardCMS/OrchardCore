namespace OrchardCore.Search.Lucene.Models;

public sealed class LuceneIndexMetadata
{
    public string AnalyzerName { get; set; }

    public LuceneIndexMap IndexMappings { get; set; }
}
