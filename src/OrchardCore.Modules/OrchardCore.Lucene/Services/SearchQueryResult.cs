using System.Collections.Generic;
using OrchardCore.ContentManagement;

namespace OrchardCore.Lucene.Services
{
    public class SearchQueryResult
    {
        public IList<ContentItem> ContentItems { get; set; }
        public IList<string> ContentItemIds { get; set; }
    }
}
