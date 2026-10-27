namespace OrchardCore.Secrets;

/// <summary>
/// Defines where the certificate of an <see cref="X509Secret"/> is loaded from.
/// </summary>
public enum X509SecretSource
{
    /// <summary>
    /// The secret references a certificate of an operating system's certificate store,
    /// with <see cref="X509Secret.StoreLocation"/>, <see cref="X509Secret.StoreName"/> and <see cref="X509Secret.Thumbprint"/>.
    /// </summary>
    Store,

    /// <summary>
    /// The secret contains the PEM-encoded certificate, in <see cref="X509Secret.CertificatePem"/>,
    /// and optionally its PEM-encoded private key, in <see cref="X509Secret.PrivateKeyPem"/>.
    /// </summary>
    Pem,
}
