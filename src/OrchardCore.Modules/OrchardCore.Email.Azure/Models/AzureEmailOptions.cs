namespace OrchardCore.Email.Azure.Models;

public class AzureEmailOptions
{
    public string DefaultSender { get; set; }

    public string ConnectionString { get; set; }

    public bool PreventAdminSettingsOverride { get; set; }

    public bool IsEnabled { get; set; }
}
