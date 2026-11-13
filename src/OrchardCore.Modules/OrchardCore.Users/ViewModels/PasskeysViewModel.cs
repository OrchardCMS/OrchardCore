namespace OrchardCore.Users.ViewModels;

public class PasskeysViewModel
{
    public IList<PasskeyEntryViewModel> Passkeys { get; set; } = [];

    /// <summary>
    /// Gets or sets whether passkeys can be used to log in, which requires the local login.
    /// </summary>
    public bool IsLoginMethod { get; set; }

    /// <summary>
    /// Gets or sets whether the user can register another passkey.
    /// </summary>
    public bool CanAddPasskey { get; set; }

    /// <summary>
    /// Gets or sets whether passkeys can be used as a two-factor authentication method.
    /// </summary>
    public bool IsTwoFactorMethod { get; set; }
}
