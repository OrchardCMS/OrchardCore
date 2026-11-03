namespace OrchardCore.Users.ViewModels;

public class PasskeysViewModel
{
    public IList<PasskeyEntryViewModel> Passkeys { get; set; } = [];

    /// <summary>
    /// Gets or sets whether passkeys can be used as a two-factor authentication method.
    /// </summary>
    public bool IsTwoFactorMethod { get; set; }
}
