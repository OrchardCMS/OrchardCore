namespace OrchardCore.Sms.Azure.Models;

public class AzureSmsSettings
{
    public bool IsEnabled { get; set; }

    public string ConnectionString { get; set; }

    public string PhoneNumber { get; set; }
}
