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
    private readonly RSASecret _encryptionSecret;
    private readonly RSASecret _signingSecret;

    public SecretHybridProtector(RSASecret encryptionSecret, RSASecret signingSecret)
    {
        _encryptionSecret = encryptionSecret;
        _signingSecret = signingSecret;
    }

    public async Task<string> ProtectAsync(string plaintext, DateTimeOffset? expiration = null)
    {
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
        rsaEncryptor.ImportRSAPublicKey(_encryptionSecret.PublicKeyAsBytes(), out _);
        var rsaEncryptedAesKey = rsaEncryptor.Encrypt(aes.Key, RSAEncryptionPadding.Pkcs1);

        // The private key is used for signing, the matching public key will have to be used for verification.
        using var rsaSigner = RSAGenerator.GenerateRSASecurityKey(2048);
        rsaSigner.ImportRSAPrivateKey(_signingSecret.PrivateKeyAsBytes(), out _);
        var signature = rsaSigner.SignData(encrypted, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        var envelope = new SecretHybridEnvelope
        {
            Key = Convert.ToBase64String(rsaEncryptedAesKey),
            Iv = Convert.ToBase64String(aes.IV),
            ProtectedData = Convert.ToBase64String(encrypted),
            Signature = Convert.ToBase64String(signature),
            EncryptionSecret = _encryptionSecret.Name,
            SigningSecret = _signingSecret.Name,
        };

        var serialized = JsonConvert.SerializeObject(envelope);
        var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(serialized));

        return encoded;
    }
}
