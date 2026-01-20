using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Search.ViewModels;

public class SearchSettingsViewModel
{
    public string DefaultIndexProfileName { get; set; }

    public string Placeholder { get; set; }

    public string PageTitle { get; set; }

    [BindNever]
    public IList<SelectListItem> Indexes { get; set; }
}
