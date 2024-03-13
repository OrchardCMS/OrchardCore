namespace OrchardCore.Email.Azure.Models;

public class AzureEmailOptions
{
    public bool IsEnabled { get; set; }

    public string DefaultSender { get; set; }

    public string ConnectionString { get; set; }

    public bool ConfigurationExists()
        => !string.IsNullOrWhiteSpace(DefaultSender) && !string.IsNullOrWhiteSpace(ConnectionString);
}
