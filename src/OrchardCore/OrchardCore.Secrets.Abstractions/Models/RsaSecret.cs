using System;

namespace OrchardCore.Secrets.Models;

public class RsaSecret : Secret
{
    public RsaSecretType KeyType { get; set; }
    public string PublicKey { get; set; }
    public string PrivateKey { get; set; }

    public byte[] PublicKeyAsBytes() =>
        !String.IsNullOrEmpty(PublicKey) ? Convert.FromBase64String(PublicKey) : Array.Empty<byte>();

    public byte[] PrivateKeyAsBytes() =>
        !String.IsNullOrEmpty(PrivateKey) ? Convert.FromBase64String(PrivateKey) : Array.Empty<byte>();
}
