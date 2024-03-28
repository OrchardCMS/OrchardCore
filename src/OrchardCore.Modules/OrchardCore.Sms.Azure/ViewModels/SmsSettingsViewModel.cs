using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Sms.Azure.ViewModels;

public class SmsSettingsViewModel : SmsSettingsBaseViewModel
{
    [BindNever]
    public SelectListItem[] Providers { get; set; }
}
