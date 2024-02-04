namespace OrchardCore.Email.Azure.ViewModels;

public class AzureEmailSettingsViewModel
{
    public string DefaultSender { get; set; }

    public string ConnectionString { get; set; }

    public bool IsConfigured { get; set; }
}
