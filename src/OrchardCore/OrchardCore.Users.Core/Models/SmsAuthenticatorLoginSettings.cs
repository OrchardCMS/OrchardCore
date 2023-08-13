using System.ComponentModel;

namespace OrchardCore.Users.Models;

public class SmsAuthenticatorLoginSettings
{
    [DefaultValue(true)]
    public bool AllowChangePhoneNumber { get; set; } = true;

    public string Body { get; set; }
}
