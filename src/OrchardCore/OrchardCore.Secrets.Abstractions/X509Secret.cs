using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace OrchardCore.Secrets;

/// <summary>
/// Represents a secret holding an X.509 certificate, either referenced from a certificate store
/// or contained in the secret as PEM-encoded certificate material.
/// </summary>
public class X509Secret : ISecret
{
    /// <summary>
    /// Gets or sets where the certificate is loaded from.
    /// </summary>
    public X509SecretSource Source { get; set; } = X509SecretSource.Store;

    /// <summary>
    /// Gets or sets the certificate store location, when <see cref="Source"/> is <see cref="X509SecretSource.Store"/>.
    /// </summary>
    public StoreLocation StoreLocation { get; set; } = StoreLocation.CurrentUser;

    /// <summary>
    /// Gets or sets the certificate store name, when <see cref="Source"/> is <see cref="X509SecretSource.Store"/>.
    /// </summary>
    public StoreName StoreName { get; set; } = StoreName.My;

    /// <summary>
    /// Gets or sets the certificate thumbprint, when <see cref="Source"/> is <see cref="X509SecretSource.Store"/>.
    /// </summary>
    public string Thumbprint { get; set; }

    /// <summary>
    /// Gets or sets the PEM-encoded certificate, when <see cref="Source"/> is <see cref="X509SecretSource.Pem"/>.
    /// It can be followed by the certificates of its chain, the first certificate being the one that is loaded.
    /// </summary>
    public string CertificatePem { get; set; }

    /// <summary>
    /// Gets or sets the unencrypted PEM-encoded private key of the certificate, when <see cref="Source"/>
    /// is <see cref="X509SecretSource.Pem"/>, or <see langword="null"/> when only the certificate is stored.
    /// </summary>
    public string PrivateKeyPem { get; set; }

    /// <summary>
    /// Creates a secret containing PEM-encoded certificate material.
    /// </summary>
    /// <param name="certificatePem">
    /// The PEM-encoded certificate, optionally followed by the certificates of its chain and by its private key.
    /// </param>
    /// <param name="privateKeyPem">The PEM-encoded private key, when it is not part of <paramref name="certificatePem"/>.</param>
    /// <returns>
    /// A secret whose <see cref="CertificatePem"/> only contains the certificates, and whose <see cref="PrivateKeyPem"/>
    /// contains the first private key found in either value.
    /// </returns>
    public static X509Secret FromPem(string certificatePem, string privateKeyPem = null)
    {
        var certificates = new StringBuilder();
        string privateKey = null;

        foreach (var pem in new[] { certificatePem, privateKeyPem })
        {
            var data = pem.AsSpan();

            while (PemEncoding.TryFind(data, out var fields))
            {
                var label = data[fields.Label];

                if (label is "CERTIFICATE")
                {
                    certificates.Append(data[fields.Location]).Append('\n');
                }
                else if (privateKey is null && label.EndsWith("PRIVATE KEY", StringComparison.Ordinal))
                {
                    privateKey = data[fields.Location].ToString() + '\n';
                }

                data = data[fields.Location.End..];
            }
        }

        return new X509Secret
        {
            Source = X509SecretSource.Pem,
            CertificatePem = certificates.Length > 0 ? certificates.ToString() : null,
            PrivateKeyPem = privateKey,
        };
    }

    /// <summary>
    /// Loads the X.509 certificate from the certificate store or from the PEM-encoded certificate material.
    /// </summary>
    /// <returns>The X.509 certificate if it could be loaded, otherwise null.</returns>
    public X509Certificate2 GetCertificate()
    {
        return Source == X509SecretSource.Pem
            ? GetCertificateFromPem()
            : GetCertificateFromStore();
    }

    private X509Certificate2 GetCertificateFromPem()
    {
        if (string.IsNullOrWhiteSpace(CertificatePem))
        {
            return null;
        }

        try
        {
            return string.IsNullOrWhiteSpace(PrivateKeyPem)
                ? X509Certificate2.CreateFromPem(CertificatePem)
                : X509Certificate2.CreateFromPem(CertificatePem, PrivateKeyPem);
        }
        catch (Exception ex) when (ex is CryptographicException or ArgumentException)
        {
            return null;
        }
    }

    private X509Certificate2 GetCertificateFromStore()
    {
        if (string.IsNullOrEmpty(Thumbprint))
        {
            return null;
        }

        try
        {
            using var store = new X509Store(StoreName, StoreLocation);
            store.Open(OpenFlags.ReadOnly);

            var certificates = store.Certificates.Find(
                X509FindType.FindByThumbprint,
                Thumbprint,
                validOnly: false);

            return certificates.Count > 0 ? certificates[0] : null;
        }
        catch
        {
            return null;
        }
    }
}
