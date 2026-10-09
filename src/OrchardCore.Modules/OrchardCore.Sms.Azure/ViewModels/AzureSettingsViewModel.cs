using OrchardCore.Secrets;
using OrchardCore.Sms.ViewModels;

namespace OrchardCore.Sms.Azure.ViewModels;

public class AzureSettingsViewModel : SmsSettingsBaseViewModel
{
    public bool IsEnabled { get; set; }

    public SecretInputViewModel ConnectionString { get; set; } = new();

    public string PhoneNumber { get; set; }
}
