using System.Security.Cryptography.X509Certificates;

namespace OrchardCore.Secrets;

/// <summary>
/// Represents a secret that references an X.509 certificate from the certificate store.
/// </summary>
public class X509Secret : ISecret
{
    /// <summary>
    /// Gets or sets the certificate store location.
    /// </summary>
    public StoreLocation StoreLocation { get; set; } = StoreLocation.CurrentUser;

    /// <summary>
    /// Gets or sets the certificate store name.
    /// </summary>
    public StoreName StoreName { get; set; } = StoreName.My;

    /// <summary>
    /// Gets or sets the certificate thumbprint.
    /// </summary>
    public string Thumbprint { get; set; }

    /// <summary>
    /// Loads the X.509 certificate from the certificate store.
    /// </summary>
    /// <returns>The X.509 certificate if found, otherwise null.</returns>
    public X509Certificate2 GetCertificate()
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
