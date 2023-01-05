using System.Collections.Generic;

namespace OrchardCore.Search.Elasticsearch.ViewModels
{
    public class ElasticSettingsViewModel
    {
        public string Analyzer { get; set; }
        public string SearchIndex { get; set; }
        public IEnumerable<string> SearchIndexes { get; set; }
        public string SearchFields { get; set; }
        public bool AllowElasticQueryStringQueryInSearch { get; set; }
    }
}
