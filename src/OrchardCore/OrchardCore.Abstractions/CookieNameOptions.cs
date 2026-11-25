namespace OrchardCore;

/// <summary>
/// The names of the cookies used by a tenant, bound from the <c>OrchardCore:Cookies</c> section, or from the
/// <c>OrchardCore:{TenantName}:Cookies</c> section for a single tenant.
/// </summary>
/// <remarks>
/// A name that is not configured gets its default value, built from the constants of <see cref="CookieNames"/>
/// and, when the default name depends on it, from the tenant settings.
/// </remarks>
public sealed class CookieNameOptions
{
    /// <summary>
    /// Gets or sets the name of the antiforgery cookie. Defaults to <c>__orchantiforgery_{VersionId}</c>.
    /// </summary>
    public string Antiforgery { get; set; }

    /// <summary>
    /// Gets or sets the name of the authentication cookie of the signed in users. Defaults to <c>orchauth_{TenantName}</c>.
    /// </summary>
    public string Authentication { get; set; }

    /// <summary>
    /// Gets or sets the name of the cookie identifying a user who has to confirm their email. Defaults to <c>orchemail_{TenantName}</c>.
    /// </summary>
    public string EmailConfirmation { get; set; }

    /// <summary>
    /// Gets or sets the name of the cookie used during a passkey sign in or registration. Defaults to <c>orchpasskey_{TenantName}</c>.
    /// </summary>
    public string Passkey { get; set; }

    /// <summary>
    /// Gets or sets the name of the cookie storing the identity of a user signing in with an external provider.
    /// Defaults to <c>Identity.External</c>.
    /// </summary>
    public string ExternalAuthentication { get; set; } = CookieNames.ExternalAuthentication;

    /// <summary>
    /// Gets or sets the name of the cookie storing the identity of a user signing in with two-factor authentication.
    /// Defaults to <c>Identity.TwoFactorUserId</c>.
    /// </summary>
    public string TwoFactorUserId { get; set; } = CookieNames.TwoFactorUserId;

    /// <summary>
    /// Gets or sets the name of the cookie remembering a browser on which a user has completed two-factor authentication.
    /// Defaults to <c>Identity.TwoFactorRememberMe</c>.
    /// </summary>
    public string TwoFactorRememberMe { get; set; } = CookieNames.TwoFactorRememberMe;

    /// <summary>
    /// Gets or sets the name of the cookie carrying the notifications to the next request. Defaults to <c>orch_notify</c>.
    /// </summary>
    public string Notify { get; set; } = CookieNames.Notify;

    /// <summary>
    /// Gets or sets the name of the cookie storing the MVC temp data. Defaults to <c>.AspNetCore.Mvc.CookieTempDataProvider</c>.
    /// </summary>
    public string TempData { get; set; } = CookieNames.TempData;

    /// <summary>
    /// Gets or sets the name of the cookie storing the culture selected by a visitor of the site. Defaults to <c>.AspNetCore.Culture</c>.
    /// </summary>
    public string Culture { get; set; } = CookieNames.Culture;

    /// <summary>
    /// Gets or sets the name of the cookie storing the tracking consent of a visitor. Defaults to <c>.AspNetCore.Consent</c>.
    /// </summary>
    public string Consent { get; set; } = CookieNames.Consent;

    /// <summary>
    /// Gets or sets the name of the cookie storing the culture selected in the admin. Defaults to <c>admin_culture_{VersionId}</c>.
    /// </summary>
    public string AdminCulture { get; set; }

    /// <summary>
    /// Gets or sets the name of the cookie storing the layout selected for the admin lists. Defaults to <c>orchardcore_admin_list_layout</c>.
    /// </summary>
    public string AdminListLayout { get; set; } = CookieNames.AdminListLayout;

    /// <summary>
    /// Gets or sets the name of the cookie storing the admin theme mode. Defaults to <c>{TenantName}-admintheme</c>.
    /// </summary>
    public string AdminTheme { get; set; }

    /// <summary>
    /// Gets or sets the name of the cookie storing the scheme selected in the admin theme. Defaults to <c>{TenantName}-TheAdmin-scheme</c>.
    /// </summary>
    public string AdminScheme { get; set; }

    /// <summary>
    /// Gets or sets the name of the cookie storing the admin preferences. Defaults to <c>{TenantName}-adminPreferences</c>.
    /// </summary>
    public string AdminPreferences { get; set; }
}
