namespace OrchardCore.Users.Models;

/// <summary>
/// The settings of the pages where users manage their security: the Security page, and the pages it links to,
/// such as the setup of an authenticator app or the recovery codes.
/// </summary>
public class UserSecuritySettings
{
    /// <summary>
    /// Gets or sets whether the security pages are rendered with the site theme instead of the admin theme.
    /// </summary>
    public bool UseSiteTheme { get; set; }
}
