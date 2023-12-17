using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OrchardCore.Secrets.Models;

namespace OrchardCore.Secrets.Services;

public class SecretHybridProtector : ISecretProtector
{
    private readonly ISecretService _secretService;
    private readonly string _encryptionSecret;
    private readonly string _signingSecret;

    private RSASecret _encryptionRSASecret;
    private RSASecret _signingRSASecret;

    public SecretHybridProtector(ISecretService secretService, string purpose = null)
    {
        _secretService = secretService;

        if (purpose is not null)
        {
            _encryptionSecret = $"{purpose}.Encryption";
            _signingSecret = $"{purpose}.Signing";
        }
    }

    public async Task<string> ProtectAsync(string plaintext, DateTimeOffset? expiration = null)
    {
        if (_encryptionSecret is null)
        {
            throw new InvalidOperationException("This protector can't be used for encryption.");
        }

        _encryptionRSASecret ??= await _secretService.GetSecretAsync<RSASecret>(_encryptionSecret)
            ?? throw new InvalidOperationException($"Secret '{_encryptionSecret}' not found.");

        _signingRSASecret ??= await _secretService.GetSecretAsync<RSASecret>(_signingSecret)
            ?? throw new InvalidOperationException($"Secret '{_signingSecret}' not found.");

        // The private key is needed for the signature.
        if (_signingRSASecret.KeyType != RSAKeyType.PublicPrivate)
        {
            throw new InvalidOperationException($"Secret '{_signingSecret}' cannot be used for signing.");
        }

        expiration ??= DateTimeOffset.MaxValue;
        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);

        // Prepend the expiration time (as a 64-bit UTC tick count).
        var plaintextWithHeader = new byte[checked(8 + plaintextBytes.Length)];
        BitHelpers.WriteUInt64(plaintextWithHeader, 0, (ulong)expiration.Value.UtcTicks);
        Buffer.BlockCopy(plaintextBytes, 0, plaintextWithHeader, 8, plaintext.Length);

        byte[] encrypted;
        using var aes = Aes.Create();
        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using (var msEncrypt = new MemoryStream())
        {
            using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
            await csEncrypt.WriteAsync(plaintextWithHeader, 0, plaintextWithHeader.Length);
            await csEncrypt.FlushFinalBlockAsync();
            encrypted = msEncrypt.ToArray();
        }

        // The public key is used for encryption, the matching private key will have to be used for decryption.
        using var rsaEncryptor = RSAGenerator.GenerateRSASecurityKey(2048);
        rsaEncryptor.ImportRSAPublicKey(_encryptionRSASecret.PublicKeyAsBytes(), out _);
        var rsaEncryptedAesKey = rsaEncryptor.Encrypt(aes.Key, RSAEncryptionPadding.Pkcs1);

        // The private key is used for signing, the matching public key will have to be used for verification.
        using var rsaSigner = RSAGenerator.GenerateRSASecurityKey(2048);
        rsaSigner.ImportRSAPrivateKey(_signingRSASecret.PrivateKeyAsBytes(), out _);
        var signature = rsaSigner.SignData(encrypted, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        var envelope = new SecretHybridEnvelope
        {
            Key = Convert.ToBase64String(rsaEncryptedAesKey),
            Iv = Convert.ToBase64String(aes.IV),
            ProtectedData = Convert.ToBase64String(encrypted),
            Signature = Convert.ToBase64String(signature),
            EncryptionSecret = _encryptionSecret,
            SigningSecret = _signingSecret,
        };

        var serialized = JsonConvert.SerializeObject(envelope);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(serialized));
    }

    public async Task<(string Plaintext, DateTimeOffset Expiration)> UnprotectAsync(string protectedData)
    {
        var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(protectedData));
        var envelope = JsonConvert.DeserializeObject<SecretHybridEnvelope>(decoded);

        var encryptionRsaSecret = await _secretService.GetSecretAsync<RSASecret>(envelope.EncryptionSecret)
            ?? throw new InvalidOperationException($"'{envelope.EncryptionSecret}' secret not found.");

        // The private key is needed for decryption.
        if (encryptionRsaSecret.KeyType != RSAKeyType.PublicPrivate)
        {
            throw new InvalidOperationException($"Secret '{encryptionRsaSecret.Name}' cannot be used for decryption.");
        }

        var signingRsaSecret = await _secretService.GetSecretAsync<RSASecret>(envelope.SigningSecret)
            ?? throw new InvalidOperationException($"'{envelope.SigningSecret}' secret not found.");

        var protectedBytes = Convert.FromBase64String(envelope.ProtectedData);
        var signatureBytes = Convert.FromBase64String(envelope.Signature);

        // The private key has been used for signing, the matching public key should be used for verification.
        using var rsaSigner = RSAGenerator.GenerateRSASecurityKey(2048);
        rsaSigner.ImportRSAPublicKey(signingRsaSecret.PublicKeyAsBytes(), out _);
        if (!rsaSigner.VerifyData(protectedBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
        {
            throw new CryptographicException("Could not verify signature.");
        }

        // The public key has been used for encryption, the matching private key should be used for decryption.
        using var rsaDecrypt = RSAGenerator.GenerateRSASecurityKey(2048);
        rsaDecrypt.ImportRSAPrivateKey(encryptionRsaSecret.PrivateKeyAsBytes(), out _);
        var aesKey = rsaDecrypt.Decrypt(Convert.FromBase64String(envelope.Key), RSAEncryptionPadding.Pkcs1);

        using var aes = Aes.Create();
        using var msDecrypt = new MemoryStream(protectedBytes);
        using var decrypt = aes.CreateDecryptor(aesKey, Convert.FromBase64String(envelope.Iv));
        using var csDecrypt = new CryptoStream(msDecrypt, decrypt, CryptoStreamMode.Read);

        var plaintextWithHeader = new byte[checked(protectedBytes.Length)];
        var decryptedBytesCount = await csDecrypt.ReadAsync(plaintextWithHeader, 0, protectedBytes.Length);
        await csDecrypt.FlushAsync();

        if (decryptedBytesCount < 8)
        {
            throw new CryptographicException("The payload is invalid, the expiration header is missing.");
        }

        var utcTicksExpiration = BitHelpers.ReadUInt64(plaintextWithHeader, 0);
        var embeddedExpiration = new DateTimeOffset(checked((long)utcTicksExpiration), TimeSpan.Zero /* UTC */);

        var plaintextBytes = new byte[decryptedBytesCount - 8];
        Buffer.BlockCopy(plaintextWithHeader, 8, plaintextBytes, 0, plaintextBytes.Length);

        return (Encoding.UTF8.GetString(plaintextBytes), embeddedExpiration);
    }
}
