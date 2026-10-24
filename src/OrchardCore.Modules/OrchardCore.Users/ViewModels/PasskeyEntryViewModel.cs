namespace OrchardCore.Users.ViewModels;

public class PasskeyEntryViewModel
{
    /// <summary>
    /// Gets or sets the credential identifier, encoded with Base64Url.
    /// </summary>
    public string Id { get; set; }

    public string Name { get; set; }

    public DateTime CreatedUtc { get; set; }

    public DateTime? LastUsedUtc { get; set; }

    /// <summary>
    /// Gets or sets whether the passkey is backed up and synced across devices.
    /// </summary>
    public bool IsBackedUp { get; set; }
}
