namespace OrchardCore.Users.Models;

/// <summary>
/// Represents a passkey (WebAuthn credential) registered by a user.
/// </summary>
public class UserPasskey
{
    /// <summary>
    /// Gets or sets the credential identifier assigned by the authenticator.
    /// </summary>
    public byte[] CredentialId { get; set; }

    /// <summary>
    /// Gets or sets the COSE-encoded public key of the credential.
    /// </summary>
    public byte[] PublicKey { get; set; }

    /// <summary>
    /// Gets or sets the name the user gave to the passkey.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the date and time, in UTC, at which the passkey was registered.
    /// </summary>
    public DateTime CreatedUtc { get; set; }

    /// <summary>
    /// Gets or sets the date and time, in UTC, at which the passkey was last used to sign in.
    /// </summary>
    public DateTime? LastUsedUtc { get; set; }

    /// <summary>
    /// Gets or sets the signature counter reported by the authenticator.
    /// </summary>
    public uint SignCount { get; set; }

    /// <summary>
    /// Gets or sets the transports supported by the authenticator, such as <c>internal</c>, <c>usb</c> or <c>hybrid</c>.
    /// </summary>
    public string[] Transports { get; set; }

    /// <summary>
    /// Gets or sets whether the user was verified, with a biometric or a PIN, when the passkey was last used.
    /// </summary>
    public bool IsUserVerified { get; set; }

    /// <summary>
    /// Gets or sets whether the passkey can be backed up and synced across devices.
    /// </summary>
    public bool IsBackupEligible { get; set; }

    /// <summary>
    /// Gets or sets whether the passkey is currently backed up.
    /// </summary>
    public bool IsBackedUp { get; set; }

    /// <summary>
    /// Gets or sets the attestation object returned when the passkey was registered.
    /// </summary>
    public byte[] AttestationObject { get; set; }

    /// <summary>
    /// Gets or sets the client data returned when the passkey was registered.
    /// </summary>
    public byte[] ClientDataJson { get; set; }
}
