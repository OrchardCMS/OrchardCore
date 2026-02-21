namespace OrchardCore.Secrets.Models;

/// <summary>
/// Represents encrypted secret data for export/import.
/// Uses hybrid encryption: RSA encrypts a random AES key, AES encrypts the actual data.
/// </summary>
public class EncryptedSecretData
{
    /// <summary>
    /// Gets or sets the AES key encrypted with RSA public key (Base64).
    /// </summary>
    public string EncryptedKey { get; set; }

    /// <summary>
    /// Gets or sets the secret data encrypted with AES (Base64).
    /// </summary>
    public string EncryptedData { get; set; }

    /// <summary>
    /// Gets or sets the AES initialization vector (Base64).
    /// </summary>
    public string IV { get; set; }
}
