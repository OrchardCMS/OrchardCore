namespace OrchardCore.Themes.TheAdmin;

/// <summary>
/// The names of the cookies used by TheAdmin theme, bound from the <c>OrchardCore:Cookies:TheAdmin</c> section, or
/// from the <c>OrchardCore:{TenantName}:Cookies:TheAdmin</c> section for a single tenant.
/// </summary>
/// <remarks>
/// A name that is not configured, or is empty, gets its default value.
/// </remarks>
public sealed class TheAdminCookieOptions
{
    /// <summary>
    /// The suffix of the default scheme cookie name, following the tenant name.
    /// </summary>
    public const string DefaultSchemeSuffix = "-TheAdmin-scheme";

    /// <summary>
    /// Gets or sets the name of the cookie holding the scheme a user picked. Defaults to <c>{TenantName}-TheAdmin-scheme</c>.
    /// </summary>
    public string Scheme { get; set; }
}