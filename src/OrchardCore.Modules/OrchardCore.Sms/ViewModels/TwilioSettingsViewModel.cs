using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Sms.ViewModels;

public class TwilioSettingsViewModel : SmsSettingsBaseViewModel
{
    public string PhoneNumber { get; set; }

    public string AccountSID { get; set; }

    public string AuthToken { get; set; }

    [BindNever]
    public bool HasAuthToken { get; set; }
}
