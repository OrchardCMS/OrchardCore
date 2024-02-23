using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Environment.Shell.Models;

namespace OrchardCore.Tenants.ViewModels
{
    public class FeatureProfilesIndexViewModel
    {
        public List<FeatureProfileEntry> FeatureProfiles { get; set; }
        public dynamic Pager { get; set; }
        public ContentOptions Options { get; set; } = new ContentOptions();
    }

    public class FeatureProfileEntry
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public FeatureProfile FeatureProfile { get; set; }

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
