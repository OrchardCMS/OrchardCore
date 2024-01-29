using System.Collections.Generic;

namespace OrchardCore.Search.Lucene.ViewModels
{
    public class LuceneSettingsViewModel
    {
        public string Analyzer { get; set; }
        public string SearchIndex { get; set; }
        public IEnumerable<string> SearchIndexes { get; set; }
        public string SearchFields { get; set; }
        public bool AllowLuceneQueriesInSearch { get; set; }
    }
}
