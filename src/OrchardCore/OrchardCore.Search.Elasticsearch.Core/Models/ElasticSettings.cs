using System;
using OrchardCore.Contents.Indexing;

namespace OrchardCore.Search.Elasticsearch.Core.Models
{
    public class ElasticSettings
    {
        public static readonly string[] FullTextField = [IndexingConstants.FullTextKey];

        [Obsolete("This property will be removed in future releases.")]
        public const string StandardAnalyzer = "standardanalyzer";

        public string SearchIndex { get; set; }

        public string DefaultQuery { get; set; }

        public string[] DefaultSearchFields { get; set; } = FullTextField;

        [Obsolete("Instead use SearchType property.")]
        public bool AllowElasticQueryStringQueryInSearch { get; set; } = false;

        public string SearchType { get; set; }
    }
}
