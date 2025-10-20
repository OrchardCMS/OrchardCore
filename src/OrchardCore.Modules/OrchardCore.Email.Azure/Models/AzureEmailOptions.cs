namespace OrchardCore.Email.Azure.Models;

public class AzureEmailOptions
{
    public bool IsEnabled { get; set; }

    public string DefaultSender { get; set; }

    public string ConnectionString { get; set; }

    public string CredentialName { get; set; }

    public Uri Endpoint { get; set; }

    public bool ConfigurationExists()
    {
        if (string.IsNullOrWhiteSpace(DefaultSender))
        {
            return false;
        }

        if (Endpoint is not null)
        {
            return true;
        }

        return !string.IsNullOrWhiteSpace(ConnectionString);
    }
}
