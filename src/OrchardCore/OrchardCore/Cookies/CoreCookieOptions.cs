namespace OrchardCore.Cookies;

/// <summary>
/// The names of the cookies used by the Orchard Core framework, bound from the <c>OrchardCore:Cookies:Core</c>
/// section, or from the <c>OrchardCore:{TenantName}:Cookies:Core</c> section for a single tenant.
/// </summary>
/// <remarks>
/// A name that is not configured, or is empty, gets its default value.
/// </remarks>
public sealed class CoreCookieOptions
{
    /// <summary>
    /// The prefix of the default antiforgery cookie name, followed by the tenant version identifier.
    /// </summary>
    public const string DefaultAntiforgeryPrefix = "__orchantiforgery_";

    /// <summary>
    /// The default name of the cookie storing the MVC temp data.
    /// </summary>
    public const string DefaultTempData = ".AspNetCore.Mvc.CookieTempDataProvider";

    /// <summary>
    /// The default name of the cookie storing the tracking consent of a visitor.
    /// </summary>
    public const string DefaultConsent = ".AspNet.Consent";

    /// <summary>
    /// Gets or sets the name of the antiforgery cookie. Defaults to <c>__orchantiforgery_{VersionId}</c>.
    /// </summary>
    public string Antiforgery { get; set; }

    /// <summary>
    /// Gets or sets the name of the cookie storing the MVC temp data. Defaults to <c>.AspNetCore.Mvc.CookieTempDataProvider</c>.
    /// </summary>
    public string TempData { get; set; }

    /// <summary>
    /// Gets or sets the name of the cookie storing the tracking consent of a visitor. Defaults to <c>.AspNet.Consent</c>.
    /// </summary>
    public string Consent { get; set; }
}
