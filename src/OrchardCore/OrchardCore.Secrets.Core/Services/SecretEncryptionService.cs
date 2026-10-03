using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using OrchardCore.Secrets.Models;

namespace OrchardCore.Secrets.Services;

/// <summary>
/// Default implementation of <see cref="ISecretEncryptionService"/> using hybrid RSA+AES encryption.
/// </summary>
public class SecretEncryptionService : ISecretEncryptionService
{
    private readonly ISecretManager _secretManager;
    private readonly IEnumerable<ISecretTypeProvider> _providers;

    public SecretEncryptionService(
        ISecretManager secretManager,
        IEnumerable<ISecretTypeProvider> providers)
    {
        _secretManager = secretManager;
        _providers = providers;
    }

    /// <inheritdoc />
    public async Task<EncryptedSecretData> EncryptAsync(ISecret secret, string encryptionKeyName, SecretInfo info)
    {
        ArgumentNullException.ThrowIfNull(secret);
        ArgumentException.ThrowIfNullOrWhiteSpace(encryptionKeyName);
        ArgumentNullException.ThrowIfNull(info);

        // Get the encryption key (can be RsaKeySecret or X509Secret)
        using var rsa = await GetRsaForEncryptionAsync(encryptionKeyName);

        var provider = GetProvider(info.Type);
        if (secret.GetType() != provider.SecretType)
        {
            throw new InvalidOperationException("The secret type does not match the export metadata.");
        }

        var secretJson = provider.Serialize(secret);
        var secretBytes = Encoding.UTF8.GetBytes(secretJson);
        var aesKey = RandomNumberGenerator.GetBytes(32);
        var nonce = RandomNumberGenerator.GetBytes(12);
        var tag = new byte[16];
        var ciphertext = new byte[secretBytes.Length];
        try
        {
            using var aes = new AesGcm(aesKey, tag.Length);
            aes.Encrypt(nonce, secretBytes, ciphertext, tag, GetAssociatedData(info));

            return new EncryptedSecretData
            {
                Version = 1,
                EncryptedKey = Convert.ToBase64String(rsa.Encrypt(aesKey, RSAEncryptionPadding.OaepSHA256)),
                EncryptedData = Convert.ToBase64String(ciphertext),
                IV = Convert.ToBase64String(nonce),
                Tag = Convert.ToBase64String(tag),
            };
        }
        finally
        {
            CryptographicOperations.ZeroMemory(aesKey);
            CryptographicOperations.ZeroMemory(secretBytes);
        }
    }

    /// <inheritdoc />
    public async Task<ISecret> DecryptAsync(EncryptedSecretData encryptedData, string decryptionKeyName, SecretInfo info)
    {
        ArgumentNullException.ThrowIfNull(encryptedData);
        ArgumentException.ThrowIfNullOrWhiteSpace(decryptionKeyName);
        ArgumentNullException.ThrowIfNull(info);
        if (encryptedData.Version != 1)
        {
            throw new CryptographicException("Unsupported secret encryption version. Unauthenticated legacy exports must be re-exported.");
        }

        var provider = GetProvider(info.Type);

        // Get the decryption key (must have private key)
        using var rsa = await GetRsaForDecryptionAsync(decryptionKeyName);

        // Decrypt the AES key with RSA
        var encryptedKey = Convert.FromBase64String(encryptedData.EncryptedKey);
        var aesKey = rsa.Decrypt(encryptedKey, RSAEncryptionPadding.OaepSHA256);

        var ciphertext = Convert.FromBase64String(encryptedData.EncryptedData);
        var plaintext = new byte[ciphertext.Length];
        try
        {
            using var aes = new AesGcm(aesKey, 16);
            aes.Decrypt(Convert.FromBase64String(encryptedData.IV), ciphertext,
                Convert.FromBase64String(encryptedData.Tag), plaintext, GetAssociatedData(info));
            return provider.Deserialize(Encoding.UTF8.GetString(plaintext));
        }
        finally
        {
            CryptographicOperations.ZeroMemory(aesKey);
            CryptographicOperations.ZeroMemory(plaintext);
        }
    }

    private async Task<RSA> GetRsaForEncryptionAsync(string keyName)
    {
        // Try RsaKeySecret first
        var rsaSecret = await _secretManager.GetSecretAsync<RsaKeySecret>(keyName);
        if (rsaSecret != null)
        {
            if (string.IsNullOrEmpty(rsaSecret.PublicKey))
            {
                throw new InvalidOperationException($"RSA key '{keyName}' does not have a public key.");
            }

            var rsa = RSA.Create();
            rsa.ImportRSAPublicKey(Convert.FromBase64String(rsaSecret.PublicKey), out _);
            return rsa;
        }

        // Try X509Secret
        var x509Secret = await _secretManager.GetSecretAsync<X509Secret>(keyName);
        if (x509Secret != null)
        {
            using var cert = x509Secret.GetCertificate();
            if (cert == null)
            {
                throw new InvalidOperationException($"Certificate for '{keyName}' was not found in the certificate store.");
            }

            using var rsa = cert.GetRSAPublicKey();
            if (rsa == null)
            {
                throw new InvalidOperationException($"Certificate '{keyName}' does not have an RSA public key.");
            }

            // Create a copy since the cert's RSA key is tied to the cert's lifetime
            var rsaCopy = RSA.Create();
            rsaCopy.ImportRSAPublicKey(rsa.ExportRSAPublicKey(), out _);
            return rsaCopy;
        }

        throw new InvalidOperationException($"Encryption key '{keyName}' was not found. Expected RsaKeySecret or X509Secret.");
    }

    private async Task<RSA> GetRsaForDecryptionAsync(string keyName)
    {
        // Try RsaKeySecret first
        var rsaSecret = await _secretManager.GetSecretAsync<RsaKeySecret>(keyName);
        if (rsaSecret != null)
        {
            if (!rsaSecret.IncludesPrivateKey || string.IsNullOrEmpty(rsaSecret.PrivateKey))
            {
                throw new InvalidOperationException($"RSA key '{keyName}' does not have a private key for decryption.");
            }

            var rsa = RSA.Create();
            rsa.ImportRSAPrivateKey(Convert.FromBase64String(rsaSecret.PrivateKey), out _);
            return rsa;
        }

        // Try X509Secret
        var x509Secret = await _secretManager.GetSecretAsync<X509Secret>(keyName);
        if (x509Secret != null)
        {
            using var cert = x509Secret.GetCertificate();
            if (cert == null)
            {
                throw new InvalidOperationException($"Certificate for '{keyName}' was not found in the certificate store.");
            }

            if (!cert.HasPrivateKey)
            {
                throw new InvalidOperationException($"Certificate '{keyName}' does not have a private key for decryption.");
            }

            using var rsa = cert.GetRSAPrivateKey();
            if (rsa == null)
            {
                throw new InvalidOperationException($"Certificate '{keyName}' does not have an RSA private key.");
            }

            // Create a copy since the cert's RSA key is tied to the cert's lifetime
            var rsaCopy = RSA.Create();
            rsaCopy.ImportRSAPrivateKey(rsa.ExportRSAPrivateKey(), out _);
            return rsaCopy;
        }

        throw new InvalidOperationException($"Decryption key '{keyName}' was not found. Expected RsaKeySecret or X509Secret.");
    }

    private ISecretTypeProvider GetProvider(string typeName) =>
        _providers.FirstOrDefault(p => p.Name == typeName || p.SecretType.FullName == typeName)
        ?? throw new InvalidOperationException($"Unknown secret type: {typeName}");

    private static byte[] GetAssociatedData(SecretInfo info) =>
        JsonSerializer.SerializeToUtf8Bytes(new
        {
            Version = 1,
            info.Name,
            info.Store,
            info.Type,
            info.Description,
            info.ExpiresUtc,
        });
}
