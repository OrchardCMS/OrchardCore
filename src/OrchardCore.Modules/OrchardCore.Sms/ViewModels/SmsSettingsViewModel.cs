using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Sms.ViewModels;

public class SmsSettingsViewModel
{
    public string DefaultProvider { get; set; }

    [BindNever]
    public SelectListItem[] Providers { get; set; }
}
