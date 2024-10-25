namespace OrchardCore.Sms.Models;

public class TwilioSettings
{
    public bool IsEnabled { get; set; }

    public string PhoneNumber { get; set; }

    public string AccountSID { get; set; }

    public string AuthToken { get; set; }
}
