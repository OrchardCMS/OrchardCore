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

        byte[] encrypted;
        using var aes = Aes.Create();
        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using (var msEncrypt = new MemoryStream())
        {
            using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
            using (var swEncrypt = new StreamWriter(csEncrypt))
            {
                await swEncrypt.WriteAsync(plaintext);
            }

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

        expiration ??= DateTimeOffset.MaxValue;
        var expirationBytes = new byte[checked(8)];
        BitHelpers.WriteUInt64(expirationBytes, 0, (ulong)expiration.Value.UtcTicks);

        var envelope = new SecretHybridEnvelope
        {
            Key = Convert.ToBase64String(rsaEncryptedAesKey),
            Iv = Convert.ToBase64String(aes.IV),
            ProtectedData = Convert.ToBase64String(encrypted),
            Signature = Convert.ToBase64String(signature),
            EncryptionSecret = _encryptionSecret,
            SigningSecret = _signingSecret,
            Expiration = Convert.ToBase64String(expirationBytes),
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
        using var decrypt = aes.CreateDecryptor(aesKey, Convert.FromBase64String(envelope.Iv));
        using var msDecrypt = new MemoryStream(protectedBytes);
        using var csDecrypt = new CryptoStream(msDecrypt, decrypt, CryptoStreamMode.Read);
        using var srDecrypt = new StreamReader(csDecrypt);

        var plaintext = await srDecrypt.ReadToEndAsync();

        var expirationBytes = Convert.FromBase64String(envelope.Expiration);
        var utcTicksExpiration = BitHelpers.ReadUInt64(expirationBytes, 0);
        var expiration = new DateTimeOffset(checked((long)utcTicksExpiration), TimeSpan.Zero /* UTC */);

        return (plaintext, expiration);
    }
}
