namespace OrchardCore.Sms.Azure.Models;

public class AzureSmsOptions
{
    public bool IsEnabled { get; set; }

    public string PhoneNumber { get; set; }

    public string ConnectionString { get; set; }

    public Uri Endpoint { get; set; }

    public string CredentialName { get; set; }

    public bool ConfigurationExists()
    {
        if (string.IsNullOrWhiteSpace(PhoneNumber))
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
