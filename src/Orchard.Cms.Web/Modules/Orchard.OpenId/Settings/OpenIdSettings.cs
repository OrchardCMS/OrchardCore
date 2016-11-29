using System.Security.Cryptography.X509Certificates;

namespace Orchard.OpenId.Settings
{
    public class OpenIdSettings
    {
        public bool TestingModeEnabled { get; set; }
        public enum TokenFormat { JWT, Encrypted }
        public TokenFormat DefaultTokenFormat { get; set; }
        public string Authority { get; set; }
        public string Audience { get; set; }
        public StoreLocation? CertificateStoreLocation { get; set; }
        public StoreName? CertificateStoreName { get; set; }
        public string CertificateThumbPrint { get; set; }
    }
}
