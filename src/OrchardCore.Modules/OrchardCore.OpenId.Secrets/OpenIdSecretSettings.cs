namespace OrchardCore.OpenId.Secrets;

/// <summary>
/// Settings for OpenID Connect keys stored in secrets.
/// </summary>
public class OpenIdSecretSettings
{
    /// <summary>
    /// Gets or sets the name of the secret containing the signing RSA key.
    /// When set, this takes precedence over the managed certificates.
    /// </summary>
    public string SigningKeySecretName { get; set; }

    /// <summary>
    /// Gets or sets the name of the secret containing the encryption RSA key.
    /// When set, this takes precedence over the managed certificates.
    /// </summary>
    public string EncryptionKeySecretName { get; set; }
}
