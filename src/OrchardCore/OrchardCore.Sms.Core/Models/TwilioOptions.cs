namespace OrchardCore.Sms.Models;

public class TwilioOptions
{
    public string PhoneNumber { get; set; }

    public string AccountSID { get; set; }

    public string AuthToken { get; set; }
}
