namespace OrchardCore.Sms.Azure.ViewModels;

public class AzureSettingsViewModel : SmsSettingsBaseViewModel
{
    public bool IsEnabled { get; set; }

    public string ConnectionString { get; set; }

    public string PhoneNumber { get; set; }
}
