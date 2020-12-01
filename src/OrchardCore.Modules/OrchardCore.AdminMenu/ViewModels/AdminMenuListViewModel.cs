using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.AdminMenu.ViewModels
{
    public class AdminMenuListViewModel
    {
        public AdminMenuListViewModel()
        {
            Options = new ContentOptions();
        }

        public IList<AdminMenuEntry> AdminMenu { get; set; }
        public ContentOptions Options { get; set; }
        public dynamic Pager { get; set; }
    }

    public class AdminMenuEntry
    {
        public Models.AdminMenu AdminMenu { get; set; }
        public bool IsChecked { get; set; }
    }

    public class ContentOptions
    {
        public ContentOptions()
        {
            BulkAction = ViewModels.ContentsBulkAction.None;
        }

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
