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

    public SecretEncryptionService(
        ISecretManager secretManager)
    {
        _secretManager = secretManager;
    }

    /// <inheritdoc />
    public async Task<EncryptedSecretData> EncryptAsync(ISecret secret, string encryptionKeyName)
    {
        ArgumentNullException.ThrowIfNull(secret);
        ArgumentException.ThrowIfNullOrWhiteSpace(encryptionKeyName);

        // Get the encryption key (can be RsaKeySecret or X509Secret)
        using var rsa = await GetRsaForEncryptionAsync(encryptionKeyName);

        // Serialize the secret to JSON
        var secretJson = JsonSerializer.Serialize(secret, secret.GetType());
        var secretBytes = Encoding.UTF8.GetBytes(secretJson);

        // Generate a random AES key and IV
        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.GenerateKey();
        aes.GenerateIV();

        // Encrypt the secret data with AES
        byte[] encryptedData;
        using (var encryptor = aes.CreateEncryptor())
        {
            encryptedData = encryptor.TransformFinalBlock(secretBytes, 0, secretBytes.Length);
        }

        // Encrypt the AES key with RSA
        var encryptedKey = rsa.Encrypt(aes.Key, RSAEncryptionPadding.OaepSHA256);

        return new EncryptedSecretData
        {
            EncryptedKey = Convert.ToBase64String(encryptedKey),
            EncryptedData = Convert.ToBase64String(encryptedData),
            IV = Convert.ToBase64String(aes.IV),
        };
    }

    /// <inheritdoc />
    public async Task<ISecret> DecryptAsync(EncryptedSecretData encryptedData, string decryptionKeyName, string secretType)
    {
        ArgumentNullException.ThrowIfNull(encryptedData);
        ArgumentException.ThrowIfNullOrWhiteSpace(decryptionKeyName);
        ArgumentException.ThrowIfNullOrWhiteSpace(secretType);

        // Get the decryption key (must have private key)
        using var rsa = await GetRsaForDecryptionAsync(decryptionKeyName);

        // Decrypt the AES key with RSA
        var encryptedKey = Convert.FromBase64String(encryptedData.EncryptedKey);
        var aesKey = rsa.Decrypt(encryptedKey, RSAEncryptionPadding.OaepSHA256);

        // Decrypt the secret data with AES
        using var aes = Aes.Create();
        aes.KeySize = 256;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = aesKey;
        aes.IV = Convert.FromBase64String(encryptedData.IV);

        byte[] decryptedData;
        using (var decryptor = aes.CreateDecryptor())
        {
            var encryptedBytes = Convert.FromBase64String(encryptedData.EncryptedData);
            decryptedData = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
        }

        // Deserialize the secret
        var secretJson = Encoding.UTF8.GetString(decryptedData);
        var type = GetSecretType(secretType);

        return (ISecret)JsonSerializer.Deserialize(secretJson, type);
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
            var cert = x509Secret.GetCertificate();
            if (cert == null)
            {
                throw new InvalidOperationException($"Certificate for '{keyName}' was not found in the certificate store.");
            }

            var rsa = cert.GetRSAPublicKey();
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
            var cert = x509Secret.GetCertificate();
            if (cert == null)
            {
                throw new InvalidOperationException($"Certificate for '{keyName}' was not found in the certificate store.");
            }

            if (!cert.HasPrivateKey)
            {
                throw new InvalidOperationException($"Certificate '{keyName}' does not have a private key for decryption.");
            }

            var rsa = cert.GetRSAPrivateKey();
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

    private static Type GetSecretType(string typeName)
    {
        return typeName switch
        {
            nameof(TextSecret) => typeof(TextSecret),
            nameof(RsaKeySecret) => typeof(RsaKeySecret),
            nameof(X509Secret) => typeof(X509Secret),
            _ => throw new InvalidOperationException($"Unknown secret type: {typeName}"),
        };
    }
}
