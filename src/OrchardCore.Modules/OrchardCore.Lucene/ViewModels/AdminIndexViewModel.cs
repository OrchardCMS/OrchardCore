using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Lucene.ViewModels
{
    public class AdminIndexViewModel
    {
        public AdminIndexViewModel()
        {
            Options = new ContentOptions();
        }

        public IEnumerable<IndexViewModel> Indexes { get; set; }

        public ContentOptions Options { get; set; }

        [BindNever]
        public dynamic Pager { get; set; }
    }

    public class ContentOptions
    {
        public ContentOptions()
        {
            BulkAction = ViewModels.ContentsBulkAction.None;
        }

        public ContentsBulkAction BulkAction { get; set; }

        #region Lists to populate

        [BindNever]
        public List<SelectListItem> ContentsBulkAction { get; set; }

        #endregion Lists to populate
    }

    public enum ContentsBulkAction
    {
        None,
        Reset,
        Rebuild,
        Remove
    }
}
