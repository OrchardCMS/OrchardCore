using System;
using System.IO;
using System.Security.Cryptography;
using OrchardCore.Secrets.Models;

namespace OrchardCore.Secrets.Services;

public class SecretHybridUnprotector : ISecretUnprotector
{
    private readonly SecretHybridEnvelope _envelope;
    private readonly RSASecret _encryptionSecret;
    private readonly RSASecret _signingSecret;

    public SecretHybridUnprotector(SecretHybridEnvelope envelope, RSASecret encryptionSecret, RSASecret signingSecret)
    {
        _envelope = envelope;
        _encryptionSecret = encryptionSecret;
        _signingSecret = signingSecret;
    }

    public string Unprotect()
    {
        var protectedBytes = Convert.FromBase64String(_envelope.ProtectedData);
        var signatureBytes = Convert.FromBase64String(_envelope.Signature);

        // The private key has been used for signing, the matching public key should be used for verification.
        using var rsaSigner = RSAGenerator.GenerateRSASecurityKey(2048);
        rsaSigner.ImportRSAPublicKey(_signingSecret.PublicKeyAsBytes(), out _);
        if (!rsaSigner.VerifyData(protectedBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
        {
            throw new CryptographicException("Could not verify signature.");
        }

        // The public key has been used for encryption, the matching private key should be used for decryption.
        using var rsaDecrypt = RSAGenerator.GenerateRSASecurityKey(2048);
        rsaDecrypt.ImportRSAPrivateKey(_encryptionSecret.PrivateKeyAsBytes(), out _);
        var aesKey = rsaDecrypt.Decrypt(Convert.FromBase64String(_envelope.Key), RSAEncryptionPadding.Pkcs1);

        using var aes = Aes.Create();
        using var decrypt = aes.CreateDecryptor(aesKey, Convert.FromBase64String(_envelope.Iv));
        using var msDecrypt = new MemoryStream(protectedBytes);
        using var csDecrypt = new CryptoStream(msDecrypt, decrypt, CryptoStreamMode.Read);
        using var srDecrypt = new StreamReader(csDecrypt);

        var plaintext = srDecrypt.ReadToEnd();

        return plaintext;
    }

    public string Unprotect(out DateTime? expirationUtc)
    {
        expirationUtc = _envelope.ExpirationUtc;
        return Unprotect();
    }
}
