using System.ComponentModel;

namespace OrchardCore.Users.Models;

public class PhoneLoginSettings
{
    [DefaultValue(true)]
    public bool AllowChangingPhoneNumber { get; set; } = true;
}
