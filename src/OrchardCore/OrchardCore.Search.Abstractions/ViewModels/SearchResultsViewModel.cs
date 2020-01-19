using System.Collections.Generic;
using OrchardCore.ContentManagement;

namespace OrchardCore.Search.Abstractions.ViewModels
{
    public class SearchResultsViewModel
    {
        public IEnumerable<ContentItem> ContentItems { get; set; }
    }
}
