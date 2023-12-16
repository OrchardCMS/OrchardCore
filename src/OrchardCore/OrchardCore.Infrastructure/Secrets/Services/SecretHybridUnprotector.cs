using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
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

    public async Task<(string Plaintext, DateTimeOffset Expiration)> UnprotectAsync()
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
        using var msDecrypt = new MemoryStream(protectedBytes);
        using var decrypt = aes.CreateDecryptor(aesKey, Convert.FromBase64String(_envelope.Iv));
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
