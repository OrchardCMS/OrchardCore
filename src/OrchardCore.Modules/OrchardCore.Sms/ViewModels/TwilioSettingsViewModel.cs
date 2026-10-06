namespace OrchardCore.Sms.ViewModels;

public class TwilioSettingsViewModel : SmsSettingsBaseViewModel
{
    public bool IsEnabled { get; set; }

    public string PhoneNumber { get; set; }

    public string AccountSID { get; set; }

    public string AuthTokenSecretName { get; set; }

    public bool HasAuthToken { get; set; }
}
