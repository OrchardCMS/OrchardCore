using System;

namespace OrchardCore.Secrets.Models;

public class RSASecret : SecretBase
{
    private byte[] _publicKeyAsBytes;
    private byte[] _privateKeyAsBytes;

    public string PublicKey { get; set; }
    public string PrivateKey { get; set; }
    public RSAKeyType KeyType { get; set; }

    public byte[] PublicKeyAsBytes()
    {
        if (_publicKeyAsBytes is not null)
        {
            return _publicKeyAsBytes;
        }

        return _publicKeyAsBytes = KeyAsBytes(PublicKey);
    }

    public byte[] PrivateKeyAsBytes()
    {
        if (_privateKeyAsBytes is not null)
        {
            return _privateKeyAsBytes;
        }

        return _privateKeyAsBytes = KeyAsBytes(PrivateKey);
    }

    public static byte[] KeyAsBytes(string key) =>
        !string.IsNullOrEmpty(key) ? Convert.FromBase64String(key) : [];
}
