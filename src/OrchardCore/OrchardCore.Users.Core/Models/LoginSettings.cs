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

    /// <summary>
    /// The default ISO 3166-1 alpha-2 region code for phone number input (e.g., "US").
    /// When set, the phone input country picker will default to this region.
    /// </summary>
    public string DefaultPhoneRegion { get; set; }
}
