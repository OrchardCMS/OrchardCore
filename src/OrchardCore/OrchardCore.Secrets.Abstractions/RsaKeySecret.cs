namespace OrchardCore.Secrets;

/// <summary>
/// A secret containing RSA key material for cryptographic operations.
/// </summary>
public class RsaKeySecret : ISecret
{
    /// <summary>
    /// Gets or sets the RSA public key in Base64 format.
    /// </summary>
    public string PublicKey { get; set; }

    /// <summary>
    /// Gets or sets the RSA private key in Base64 format.
    /// This should only be set if <see cref="IncludesPrivateKey"/> is true.
    /// </summary>
    public string PrivateKey { get; set; }

    /// <summary>
    /// Gets or sets whether this key includes the private key.
    /// </summary>
    public bool IncludesPrivateKey { get; set; }

    /// <summary>
    /// Gets or sets the key size in bits.
    /// </summary>
    public int KeySize { get; set; } = 2048;
}
