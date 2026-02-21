using OrchardCore.Sms.ViewModels;

namespace OrchardCore.Sms.Azure.ViewModels;

public class AzureSettingsViewModel : SmsSettingsBaseViewModel
{
    public bool IsEnabled { get; set; }

    public string ConnectionStringSecretName { get; set; }

    public string PhoneNumber { get; set; }

    public bool HasConnectionString { get; set; }
}
