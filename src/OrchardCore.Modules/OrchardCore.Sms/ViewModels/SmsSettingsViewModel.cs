using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Sms.ViewModels;

public class SmsSettingsBaseViewModel
{
    public string DefaultProvider { get; set; }
}

public class SmsSettingsViewModel : SmsSettingsBaseViewModel
{
    [BindNever]
    public SelectListItem[] Providers { get; set; }
}
