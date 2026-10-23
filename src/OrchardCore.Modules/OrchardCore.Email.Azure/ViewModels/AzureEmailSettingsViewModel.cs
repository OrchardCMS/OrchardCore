using OrchardCore.Secrets;

namespace OrchardCore.Email.Azure.ViewModels;

public class AzureEmailSettingsViewModel
{
    public bool IsEnabled { get; set; }

    [EmailAddress]
    public string DefaultSender { get; set; }

    public SecretInputViewModel ConnectionString { get; set; } = new();
}
