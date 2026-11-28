namespace OrchardCore.Localization;

/// <summary>
/// The names of the cookies used by the localization features, bound from the <c>OrchardCore:Cookies:Localization</c>
/// section, or from the <c>OrchardCore:{TenantName}:Cookies:Localization</c> section for a single tenant.
/// </summary>
/// <remarks>
/// A name that is not configured, or is empty, gets its default value.
/// </remarks>
public sealed class LocalizationCookieOptions
{
    /// <summary>
    /// The default name of the cookie storing the culture selected by a visitor.
    /// </summary>
    public const string DefaultCulture = ".AspNetCore.Culture";

    /// <summary>
    /// The prefix of the default admin culture cookie name, followed by the tenant version identifier.
    /// </summary>
    public const string DefaultAdminCulturePrefix = "admin_culture_";

    /// <summary>
    /// Gets or sets the name of the cookie storing the culture selected by a visitor, e.g. with the content culture
    /// picker. Defaults to <c>.AspNetCore.Culture</c>.
    /// </summary>
    public string Culture { get; set; }

    /// <summary>
    /// Gets or sets the name of the cookie storing the culture selected in the admin with the admin culture picker.
    /// Defaults to <c>admin_culture_{VersionId}</c>.
    /// </summary>
    public string AdminCulture { get; set; }
}
