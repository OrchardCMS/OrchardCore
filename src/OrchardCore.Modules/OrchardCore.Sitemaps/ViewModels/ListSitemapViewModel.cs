using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Sitemaps.ViewModels
{
    public class ListSitemapViewModel
    {
        public IList<SitemapListEntry> Sitemaps { get; set; }
        public ContentOptions Options { get; set; } = new ContentOptions();
        public dynamic Pager { get; set; }
    }

    public class SitemapListEntry
    {
        public string SitemapId { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; }
    }

    public class ContentOptions
    {
        public string Search { get; set; }
        public ContentsBulkAction BulkAction { get; set; }

        #region Lists to populate

        [BindNever]
        public List<SelectListItem> ContentsBulkAction { get; set; }

        #endregion Lists to populate
    }

    public enum ContentsBulkAction
    {
        None,
        Remove
    }

}
