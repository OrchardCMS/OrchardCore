namespace OrchardCore.Sms.Azure.Models;

public class AzureSmsOptions
{
    public bool IsEnabled { get; set; }

    public string PhoneNumber { get; set; }

    public string ConnectionString { get; set; }

    public bool ConfigurationExists()
        => !string.IsNullOrWhiteSpace(PhoneNumber) &&
        !string.IsNullOrWhiteSpace(ConnectionString);
}
