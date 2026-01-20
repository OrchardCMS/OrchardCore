using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.ContentLocalization.ViewModels
{
    public class LocalizationContentsAdminFilterViewModel
    {
        public bool ShowLocalizedContentTypes { get; set; }
        public string SelectedCulture { get; set; }

        [BindNever]
        public List<SelectListItem> Cultures { get; set; }
    }
}
