using System;
using System.IO;
using System.Security.Cryptography;
using OrchardCore.Secrets.Models;

namespace OrchardCore.Secrets.Services;

public class SecretDecryptor : ISecretDecryptor
{
    private readonly HybridKeyDescriptor _descriptor;
    private readonly RsaSecret _encryptionSecret;
    private readonly RsaSecret _signingSecret;

    public SecretDecryptor(HybridKeyDescriptor descriptor, RsaSecret encryptionSecret, RsaSecret signingSecret)
    {
        _descriptor = descriptor;
        _encryptionSecret = encryptionSecret;
        _signingSecret = signingSecret;
    }

    public string Decrypt(string protectedData)
    {
        var protectedBytes = Convert.FromBase64String(_descriptor.ProtectedData);
        var signatureBytes = Convert.FromBase64String(_descriptor.Signature);

        // The private key has been used for signing, the matching public key should be used for verification.
        using var rsaSigner = RsaGenerator.GenerateRsaSecurityKey(2048);
        rsaSigner.ImportRSAPublicKey(_signingSecret.PublicKeyAsBytes(), out _);

        if (!rsaSigner.VerifyData(protectedBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
        {
            throw new CryptographicException("Could not verify signature.");
        }

        // The public key has been used for encryption, the matching private key should be used for decryption.
        using var rsaDecryptor = RsaGenerator.GenerateRsaSecurityKey(2048);
        rsaDecryptor.ImportRSAPrivateKey(_encryptionSecret.PrivateKeyAsBytes(), out _);

        var aesKey = rsaDecryptor.Decrypt(Convert.FromBase64String(_descriptor.Key), RSAEncryptionPadding.Pkcs1);

        using var aes = Aes.Create();
        using var decryptor = aes.CreateDecryptor(aesKey, Convert.FromBase64String(_descriptor.Iv));
        using var msDecrypt = new MemoryStream(protectedBytes);
        using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using var srDecrypt = new StreamReader(csDecrypt);

        var plaintext = srDecrypt.ReadToEnd();

        return plaintext;
    }
}
