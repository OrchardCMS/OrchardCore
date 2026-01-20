using System.ComponentModel;

namespace OrchardCore.Users.Models;

public class IdentitySettings
{
    public IdentityUserSettings UserSettings { get; set; }
}

public class IdentityUserSettings
{
    public const string DefaultAllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._+";

    /// <summary>
    /// The list of allowed characters in the username used to validate user names.
    /// </summary>
    public string AllowedUserNameCharacters { get; set; } = DefaultAllowedUserNameCharacters;

    /// <summary>
    /// Gets or sets a flag indicating whether the application requires unique emails
    /// for its users. Defaults to false.
    /// </summary>
    [DefaultValue(true)]
    public bool RequireUniqueEmail { get; set; } = true;
}
