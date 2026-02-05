namespace OrchardCore.Email.Azure.ViewModels;

public class AzureEmailSettingsViewModel
{
    public bool IsEnabled { get; set; }

    [EmailAddress]
    public string DefaultSender { get; set; }

    public string ConnectionStringSecretName { get; set; }

    public bool HasConnectionString { get; set; }
}
