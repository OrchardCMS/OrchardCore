using System.Collections.Generic;
using Lucene.Net.Util;

namespace OrchardCore.Lucene.ViewModels
{
    public class LuceneSettingsViewModel
    {        
        public LuceneVersion Version { get; set; }
        public string Analyzer { get; set; }
        public string SearchIndex { get; set; }
        public IEnumerable<string> SearchIndexes { get; set; }
        public string SearchFields { get; set; }
    }
}
