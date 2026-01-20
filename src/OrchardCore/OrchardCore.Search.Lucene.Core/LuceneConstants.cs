using Lucene.Net.Util;

namespace OrchardCore.Search.Lucene;

public static class LuceneConstants
{
    public const string DefaultAnalyzer = "standardanalyzer";

    public const string ProviderName = "Lucene";

    public static readonly LuceneVersion DefaultVersion = LuceneVersion.LUCENE_48;
}
