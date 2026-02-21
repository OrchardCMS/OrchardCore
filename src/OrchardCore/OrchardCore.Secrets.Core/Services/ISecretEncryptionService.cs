using OrchardCore.Secrets.Models;

namespace OrchardCore.Secrets.Services;

/// <summary>
/// Service for encrypting and decrypting secrets using RSA or X509 keys.
/// </summary>
public interface ISecretEncryptionService
{
    /// <summary>
    /// Encrypts a secret using the specified encryption key.
    /// </summary>
    /// <param name="secret">The secret to encrypt.</param>
    /// <param name="encryptionKeyName">The name of the RsaKeySecret or X509Secret to use for encryption.</param>
    /// <returns>The encrypted secret data.</returns>
    Task<EncryptedSecretData> EncryptAsync(ISecret secret, string encryptionKeyName);

    /// <summary>
    /// Decrypts secret data using the specified decryption key.
    /// </summary>
    /// <param name="encryptedData">The encrypted secret data.</param>
    /// <param name="decryptionKeyName">The name of the RsaKeySecret or X509Secret to use for decryption.</param>
    /// <param name="secretType">The type name of the secret to deserialize.</param>
    /// <returns>The decrypted secret.</returns>
    Task<ISecret> DecryptAsync(EncryptedSecretData encryptedData, string decryptionKeyName, string secretType);
}
