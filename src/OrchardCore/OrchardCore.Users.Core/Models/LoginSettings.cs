using System.ComponentModel;

namespace OrchardCore.Users.Models;

public class LoginSettings
{
    public bool UseSiteTheme { get; set; }

    public bool DisableLocalLogin { get; set; }

    public bool AllowChangingUsername { get; set; }

    public bool AllowChangingEmail { get; set; }

    [DefaultValue(true)]
    public bool AllowChangingPhoneNumber { get; set; } = true;
}
