using System.Collections.Generic;
using OrchardCore.ContentManagement;

namespace OrchardCore.Contents.ViewModels
{
    public class ListContentsViewModel
    {
        public int? Page { get; set; }
        public IList<Entry> Entries { get; set; }
        
        #region Nested type: Entry

        public class Entry
        {
            public ContentItem ContentItem { get; set; }
            public ContentItemMetadata ContentItemMetadata { get; set; }
        }

        #endregion
    }

}