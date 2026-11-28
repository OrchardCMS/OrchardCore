namespace OrchardCore.Users;

/// <summary>
/// The names of the cookies used by the users features, bound from the <c>OrchardCore:Cookies:Users</c> section,
/// or from the <c>OrchardCore:{TenantName}:Cookies:Users</c> section for a single tenant.
/// </summary>
/// <remarks>
/// A name that is not configured, or is empty, gets its default value.
/// </remarks>
public sealed class UsersCookieOptions
{
    /// <summary>
    /// The prefix of the default authentication cookie name, followed by the URL encoded tenant name.
    /// </summary>
    public const string DefaultAuthenticationPrefix = "orchauth_";

    /// <summary>
    /// The prefix of the default email confirmation cookie name, followed by the URL encoded tenant name.
    /// </summary>
    public const string DefaultEmailConfirmationPrefix = "orchemail_";

    /// <summary>
    /// The prefix of the default passkey cookie name, followed by the URL encoded tenant name.
    /// </summary>
    public const string DefaultPasskeyPrefix = "orchpasskey_";

    /// <summary>
    /// The default name of the cookie storing the identity of a user signing in with an external provider.
    /// </summary>
    public const string DefaultExternalAuthentication = "Identity.External";

    /// <summary>
    /// The default name of the cookie storing the user being verified with a second factor.
    /// </summary>
    public const string DefaultTwoFactorUserId = "Identity.TwoFactorUserId";

    /// <summary>
    /// The default name of the cookie remembering a browser verified with a second factor.
    /// </summary>
    public const string DefaultTwoFactorRememberMe = "Identity.TwoFactorRememberMe";

    /// <summary>
    /// Gets or sets the name of the cookie authenticating the signed in users. Defaults to <c>orchauth_{TenantName}</c>.
    /// </summary>
    public string Authentication { get; set; }

    /// <summary>
    /// Gets or sets the name of the cookie used while confirming the email of a user. Defaults to <c>orchemail_{TenantName}</c>.
    /// </summary>
    public string EmailConfirmation { get; set; }

    /// <summary>
    /// Gets or sets the name of the cookie used while signing in with a passkey. Defaults to <c>orchpasskey_{TenantName}</c>.
    /// </summary>
    public string Passkey { get; set; }

    /// <summary>
    /// Gets or sets the name of the cookie storing the identity of a user signing in with an external provider.
    /// Defaults to <c>Identity.External</c>.
    /// </summary>
    public string ExternalAuthentication { get; set; }

    /// <summary>
    /// Gets or sets the name of the cookie storing the user being verified with a second factor.
    /// Defaults to <c>Identity.TwoFactorUserId</c>.
    /// </summary>
    public string TwoFactorUserId { get; set; }

    /// <summary>
    /// Gets or sets the name of the cookie remembering a browser verified with a second factor.
    /// Defaults to <c>Identity.TwoFactorRememberMe</c>.
    /// </summary>
    public string TwoFactorRememberMe { get; set; }
}
