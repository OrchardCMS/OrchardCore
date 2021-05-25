using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement;

namespace OrchardCore.Contents.ViewModels
{
    public class HistoryIndexViewModel
    {
        public int? Page { get; set; }


        [BindNever]
        public dynamic Header { get; set; }

        [BindNever]
        public List<IShape> ContentItems { get; set; }
        public ContentItem OldContentItem { get; set; }
        public ContentItem NewContentItem { get; set; }

        [BindNever]
        public dynamic Pager { get; set; }
    }
}
