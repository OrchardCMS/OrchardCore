using OrchardCore.Data.Documents;
using OrchardCore.Search.Models;

namespace OrchardCore.Search.Lucene.Model;

public class LuceneIndexSettings : IndexSettingsBase
{
    public string AnalyzerName { get; set; }

    public bool IndexLatest { get; set; }

    public string[] IndexedContentTypes { get; set; }

    public string Culture { get; set; }

    public bool StoreSourceData { get; set; }
}

public class LuceneIndexSettingsDocument : Document
{
    public Dictionary<string, LuceneIndexSettings> LuceneIndexSettings { get; set; } = [];
}
