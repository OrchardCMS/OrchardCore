using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Email.ViewModels;

public class EmailSettingsViewModel : EmailSettingsBaseViewModel
{
    [BindNever]
    public SelectListItem[] Providers { get; set; }
}
