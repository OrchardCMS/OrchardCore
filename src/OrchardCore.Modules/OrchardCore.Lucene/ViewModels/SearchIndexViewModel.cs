using System.Collections.Generic;
using OrchardCore.ContentManagement;
using OrchardCore.Navigation;

namespace OrchardCore.Lucene.ViewModels
{
    public class SearchIndexViewModel
    {
        public bool HasMoreResults { get; set; }
        public Pager Pager { get; set; }
        public string Query { get; set; }
        public string IndexName { get; set; }
        public IEnumerable<ContentItem> ContentItems { get; set; } 
    }
}
