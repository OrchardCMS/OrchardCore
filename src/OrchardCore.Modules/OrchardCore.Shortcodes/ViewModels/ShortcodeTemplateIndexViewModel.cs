using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Shortcodes.Models;

namespace OrchardCore.Shortcodes.ViewModels
{
    public class ShortcodeTemplateIndexViewModel
    {
        public IList<ShortcodeTemplateEntry> ShortcodeTemplates { get; set; }
        public dynamic Pager { get; set; }
        public ContentOptions Options { get; set; } = new ContentOptions();
    }

    public class ShortcodeTemplateEntry
    {
        public string Name { get; set; }
        public ShortcodeTemplate ShortcodeTemplate { get; set; }
        public bool IsChecked { get; set; }
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
