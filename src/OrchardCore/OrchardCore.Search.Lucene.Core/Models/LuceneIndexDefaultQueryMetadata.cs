using Lucene.Net.Util;

namespace OrchardCore.Search.Lucene.Models;

public sealed class LuceneIndexDefaultQueryMetadata
{
    public LuceneVersion DefaultVersion { get; set; } = LuceneConstants.DefaultVersion;

    public string QueryAnalyzerName { get; set; }

    public bool AllowLuceneQueries { get; set; }

    public string[] DefaultSearchFields { get; set; }
}
