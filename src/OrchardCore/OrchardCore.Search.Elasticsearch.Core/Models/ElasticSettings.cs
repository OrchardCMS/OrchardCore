using OrchardCore.Contents.Indexing;

namespace OrchardCore.Search.Elasticsearch.Core.Models
{
    public class ElasticSettings
    {
        public static readonly string[] FullTextField = new string[] { IndexingConstants.FullTextKey };

        public static string StandardAnalyzer = "standardanalyzer";

        public static string SimpleAnalyzer = "Simple Analyzer";

        public static string WhitespaceAnalyzer = "Whitespace Analyzer";

        public static string StopAnalyzer = "Stop Analyzer";

        public static string KeywordAnalyzer = "Keyword Analyzer";

        public static string PatternAnalyzer = "Pattern Analyzer";

        public static string LanguageAnalyzers = "Language Analyzers";

        public static string FingerprintAnalyzer = "Fingerprint Analyzer";

        public string SearchIndex { get; set; }

        public string[] DefaultSearchFields { get; set; } = FullTextField;

        public bool AllowElasticQueryStringQueryInSearch { get; set; } = false;
    }
}
