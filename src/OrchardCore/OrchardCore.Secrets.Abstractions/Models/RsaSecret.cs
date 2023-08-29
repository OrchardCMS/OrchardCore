using System;

namespace OrchardCore.Secrets.Models;

public class RsaSecret : Secret
{
    public RsaKeyType KeyType { get; set; }
    public string PublicKey { get; set; }
    public string PrivateKey { get; set; }

    public byte[] PublicKeyAsBytes() => KeyAsBytes(PublicKey);
    public byte[] PrivateKeyAsBytes() => KeyAsBytes(PrivateKey);

    public static byte[] KeyAsBytes(string key) =>
        !String.IsNullOrEmpty(key)
        ? Convert.FromBase64String(key)
        : Array.Empty<byte>();
}
