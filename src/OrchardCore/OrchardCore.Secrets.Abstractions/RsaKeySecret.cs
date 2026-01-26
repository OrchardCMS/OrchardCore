namespace OrchardCore.Secrets;

/// <summary>
/// A secret containing RSA key material for cryptographic operations.
/// </summary>
public class RsaKeySecret : SecretBase
{
    /// <summary>
    /// Gets or sets the RSA key in XML format.
    /// </summary>
    public string KeyAsXml { get; set; }

    /// <summary>
    /// Gets or sets whether this key includes the private key.
    /// </summary>
    public bool IncludesPrivateKey { get; set; }

    /// <summary>
    /// Gets or sets the key size in bits.
    /// </summary>
    public int KeySize { get; set; }
}
