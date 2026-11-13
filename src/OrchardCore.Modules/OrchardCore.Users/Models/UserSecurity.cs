namespace OrchardCore.Users.Models;

/// <summary>
/// The model of the Security page, where display drivers add the sections a user manages their security from,
/// such as two-factor authentication or passkeys.
/// </summary>
public class UserSecurity
{
    /// <summary>
    /// Gets or sets the user whose security is displayed.
    /// </summary>
    public IUser User { get; set; }
}
