using System.Collections.Generic;
using OrchardCore.ContentManagement;
using OrchardCore.Navigation;

namespace OrchardCore.Search.Abstractions.ViewModels
{
    public class SearchIndexViewModel
    {
        public string Query { get; set; }
        public string IndexName { get; set; }
        public IEnumerable<ContentItem> ContentItems { get; set; }
        public dynamic Pager { get; set; }
        public bool HasMoreResults { get; set; }
    }
}
