using OrchardCore.Contents.Indexing;

namespace OrchardCore.OpenSearch;

public static class OpenSearchConstants
{
    public const string CustomSearchType = "custom";

    public const string QueryStringSearchType = "query_string";

    public static readonly string[] FullTextField = [ContentIndexingConstants.FullTextKey];

    public const string DefaultAnalyzer = "standard";

    public const string SimpleAnalyzer = "simple";

    public const string KeywordAnalyzer = "keyword";

    public const string WhitespaceAnalyzer = "whitespace";

    public const string PatternAnalyzer = "pattern";

    public const string FingerprintAnalyzer = "fingerprint";

    public const string CustomAnalyzer = "custom";

    public const string StopAnalyzer = "stop";

    public const string ProviderName = "OpenSearch";

    public const string LastTaskIdMetadataKey = "last_task_id";
}
