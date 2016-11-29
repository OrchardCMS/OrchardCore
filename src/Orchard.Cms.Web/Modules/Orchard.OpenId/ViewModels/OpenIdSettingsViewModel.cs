
using Orchard.OpenId.Services;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using static Orchard.OpenId.Settings.OpenIdSettings;

namespace Orchard.OpenId.ViewModels
{
    public class OpenIdSettingsViewModel
    {
        public bool TestingModeEnabled { get; set; }
        public TokenFormat DefaultTokenFormat { get; set; }
        public string Authority { get; set; }
        public string Audience { get; set; }
        public StoreLocation? CertificateStoreLocation { get; set; }
        public StoreName? CertificateStoreName { get; set; }
        public string CertificateThumbPrint { get; set; }
        public IEnumerable<CertificateInfo> AvailableCertificates { get; set; }
        public string SslBaseUrl { get; set; }
    }
}
