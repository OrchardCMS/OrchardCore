using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.Navigation;

namespace Lucene.ViewModels
{
    public class SearchIndexViewModel
    {
        public PagerParameters PagerParameters { get; set; }
        public string Query { get; set; }
        public string IndexName { get; set; }
        public IEnumerable<ContentItem> ContentItems { get; set; } 
    }
}
