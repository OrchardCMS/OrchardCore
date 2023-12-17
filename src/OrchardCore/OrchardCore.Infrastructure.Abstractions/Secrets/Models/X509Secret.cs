using System.Security.Cryptography.X509Certificates;

namespace OrchardCore.Secrets.Models;

public class X509Secret : SecretBase
{
    public StoreLocation? StoreLocation { get; set; }
    public StoreName? StoreName { get; set; }
    public string Thumbprint { get; set; }
}
