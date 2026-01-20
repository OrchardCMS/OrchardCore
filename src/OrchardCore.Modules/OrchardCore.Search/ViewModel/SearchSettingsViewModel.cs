using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Search.ViewModels
{
    public class SearchSettingsViewModel
    {
        public string ProviderName { get; set; }

        public string Placeholder { get; set; }

        public string PageTitle { get; set; }

        [BindNever]
        public IList<SelectListItem> SearchServices { get; set; }
    }
}
