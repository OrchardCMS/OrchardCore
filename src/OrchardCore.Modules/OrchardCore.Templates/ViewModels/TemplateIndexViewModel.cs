using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Templates.Models;

namespace OrchardCore.Templates.ViewModels
{
    public class TemplateIndexViewModel
    {
        public IList<TemplateEntry> Templates { get; set; }
        public dynamic Pager { get; set; }
        public ContentOptions Options { get; set; } = new ContentOptions();
    }

    public class TemplateEntry
    {
        public string Name { get; set; }
        public Template Template { get; set; }
        public bool IsChecked { get; set; }
    }

    public class ContentOptions
    {
        public bool AdminTemplates { get; set; }
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
