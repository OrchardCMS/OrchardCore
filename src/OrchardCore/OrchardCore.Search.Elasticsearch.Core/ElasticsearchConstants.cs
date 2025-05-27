using OrchardCore.Contents.Indexing;

namespace OrchardCore.Search.Elasticsearch;

public static class ElasticsearchConstants
{
    public const string CustomSearchType = "custom";

    public const string QueryStringSearchType = "query_string";

    public static readonly string[] FullTextField = [ContentIndexingConstants.FullTextKey];

    public const string DefaultAnalyzer = "standard";

    public const string SimpleAnalyzer = "simple";

    public const string KeywordAnalyzer = "keyword";

    public const string WhitespaceAnalyzer = "whitespace";

    public const string PatternAnalyzer = "pattern";

    public const string LanguageAnalyzer = "language";

    public const string FingerprintAnalyzer = "fingerprint";

    public const string CustomAnalyzer = "custom";

    public const string StopAnalyzer = "stop";

    public const string ProviderName = "Elasticsearch";

    public const string LastTaskIdMetadataKey = "last_task_id";

}
