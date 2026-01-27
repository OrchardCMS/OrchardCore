using System.Security.Cryptography.X509Certificates;

namespace OrchardCore.Secrets.ViewModels;

public class X509SecretViewModel
{
    public StoreLocation StoreLocation { get; set; }

    public StoreName StoreName { get; set; }

    public string Thumbprint { get; set; }

    public bool IsNew { get; set; }

    public List<CertificateInfo> AvailableCertificates { get; set; } = [];
}

public class CertificateInfo
{
    public StoreLocation StoreLocation { get; set; }

    public StoreName StoreName { get; set; }

    public string FriendlyName { get; set; }

    public string Issuer { get; set; }

    public string Subject { get; set; }

    public DateTime NotBefore { get; set; }

    public DateTime NotAfter { get; set; }

    public string Thumbprint { get; set; }

    public bool HasPrivateKey { get; set; }

    public bool Archived { get; set; }
}
