namespace OrchardCore;

/// <summary>
/// The default names, prefixes and suffixes of the cookies used by Orchard Core.
/// </summary>
/// <remarks>
/// The names actually used by a tenant are resolved by <see cref="CookieNameOptions"/>, which can be configured
/// in the <c>OrchardCore:Cookies</c> section, or in the <c>OrchardCore:{TenantName}:Cookies</c> section for a
/// single tenant.
/// </remarks>
public static class CookieNames
{
    /// <summary>
    /// The prefix of the antiforgery cookie, followed by the tenant version identifier.
    /// </summary>
    public const string AntiforgeryPrefix = "__orchantiforgery_";

    /// <summary>
    /// The prefix of the authentication cookie of the signed in users, followed by the URL encoded tenant name.
    /// </summary>
    public const string AuthenticationPrefix = "orchauth_";

    /// <summary>
    /// The prefix of the cookie identifying a user who has to confirm their email, followed by the URL encoded tenant name.
    /// </summary>
    public const string EmailConfirmationPrefix = "orchemail_";

    /// <summary>
    /// The prefix of the cookie used during a passkey sign in or registration, followed by the URL encoded tenant name.
    /// </summary>
    public const string PasskeyPrefix = "orchpasskey_";

    /// <summary>
    /// The name of the cookie storing the identity of a user signing in with an external provider.
    /// </summary>
    public const string ExternalAuthentication = "Identity.External";

    /// <summary>
    /// The name of the cookie storing the identity of a user signing in with two-factor authentication.
    /// </summary>
    public const string TwoFactorUserId = "Identity.TwoFactorUserId";

    /// <summary>
    /// The name of the cookie remembering a browser on which a user has completed two-factor authentication.
    /// </summary>
    public const string TwoFactorRememberMe = "Identity.TwoFactorRememberMe";

    /// <summary>
    /// The name of the cookie carrying the notifications to the next request.
    /// </summary>
    public const string Notify = "orch_notify";

    /// <summary>
    /// The name of the cookie storing the MVC temp data.
    /// </summary>
    public const string TempData = ".AspNetCore.Mvc.CookieTempDataProvider";

    /// <summary>
    /// The name of the cookie storing the culture selected by a visitor of the site.
    /// </summary>
    public const string Culture = ".AspNetCore.Culture";

    /// <summary>
    /// The name of the cookie storing the tracking consent of a visitor.
    /// </summary>
    public const string Consent = ".AspNet.Consent";

    /// <summary>
    /// The prefix of the cookie storing the culture selected in the admin, followed by the tenant version identifier.
    /// </summary>
    public const string AdminCulturePrefix = "admin_culture_";

    /// <summary>
    /// The name of the cookie storing the layout selected for the admin lists.
    /// </summary>
    public const string AdminListLayout = "orchardcore_admin_list_layout";

    /// <summary>
    /// The suffix of the cookie storing the admin theme mode, preceded by the tenant name.
    /// </summary>
    public const string AdminThemeSuffix = "-admintheme";

    /// <summary>
    /// The suffix of the cookie storing the scheme selected in the admin theme, preceded by the tenant name.
    /// </summary>
    public const string AdminSchemeSuffix = "-TheAdmin-scheme";

    /// <summary>
    /// The suffix of the cookie storing the admin preferences, preceded by the tenant name.
    /// </summary>
    public const string AdminPreferencesSuffix = "-adminPreferences";
}
