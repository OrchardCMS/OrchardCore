using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using OrchardCore.Secrets.Models;

namespace OrchardCore.Secrets.Services;

public class SecretEncryptor : ISecretEncryptor
{
    private readonly RsaSecret _encryptionSecret;
    private readonly RsaSecret _signingSecret;

    public SecretEncryptor(RsaSecret encryptionSecret, RsaSecret signingSecret)
    {
        _encryptionSecret = encryptionSecret;
        _signingSecret = signingSecret;
    }

    public string Encrypt(string plainText)
    {
        byte[] encrypted;
        using var aes = Aes.Create();

        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using (var msEncrypt = new MemoryStream())
        {
            using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
            using (var swEncrypt = new StreamWriter(csEncrypt))
            {
                swEncrypt.Write(plainText);
            }

            encrypted = msEncrypt.ToArray();
        }

        // The public key is used for encryption, the matching private key will have to be used for decryption.
        using var rsaEncryptor = RsaHelper.GenerateRsaSecurityKey(2048);
        rsaEncryptor.ImportRSAPublicKey(_encryptionSecret.PublicKeyAsBytes(), out _);

        // The private key is used for signing, the matching public key will have to be used for verification.
        using var rsaSigner = RsaHelper.GenerateRsaSecurityKey(2048);
        rsaSigner.ImportRSAPrivateKey(_signingSecret.PrivateKeyAsBytes(), out _);

        var rsaEncryptedAesKey = rsaEncryptor.Encrypt(aes.Key, RSAEncryptionPadding.Pkcs1);
        var signature = rsaSigner.SignData(encrypted, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        var descriptor = new HybridKeyDescriptor
        {
            EncryptionSecretName = _encryptionSecret.Name,
            Key = Convert.ToBase64String(rsaEncryptedAesKey),
            Iv = Convert.ToBase64String(aes.IV),
            ProtectedData = Convert.ToBase64String(encrypted),
            Signature = Convert.ToBase64String(signature),
            SigningSecretName = _signingSecret.Name,
        };

        var serialized = JsonConvert.SerializeObject(descriptor);
        var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(serialized));

        return encoded;
    }
}
