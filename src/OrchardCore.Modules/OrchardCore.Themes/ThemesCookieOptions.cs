namespace OrchardCore.Themes;

/// <summary>
/// The names of the cookies used by the admin themes, bound from the <c>OrchardCore:Cookies:Themes</c> section, or
/// from the <c>OrchardCore:{TenantName}:Cookies:Themes</c> section for a single tenant.
/// </summary>
/// <remarks>
/// A name that is not configured, or is empty, gets its default value.
/// </remarks>
public sealed class ThemesCookieOptions
{
    /// <summary>
    /// The suffix of the default admin theme cookie name, following the tenant name.
    /// </summary>
    public const string DefaultAdminThemeSuffix = "-admintheme";

    /// <summary>
    /// The suffix of the default admin preferences cookie name, following the tenant name.
    /// </summary>
    public const string DefaultAdminPreferencesSuffix = "-adminPreferences";

    /// <summary>
    /// Gets or sets the name of the cookie holding the color mode a user picked with the theme toggler.
    /// Defaults to <c>{TenantName}-admintheme</c>.
    /// </summary>
    public string AdminTheme { get; set; }

    /// <summary>
    /// Gets or sets the name of the cookie holding the admin preferences of a user, e.g. the state of the admin menu.
    /// Defaults to <c>{TenantName}-adminPreferences</c>.
    /// </summary>
    public string AdminPreferences { get; set; }
}