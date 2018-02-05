using OrchardCore.OpenId.Models;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using static OrchardCore.OpenId.Settings.OpenIdSettings;

namespace OrchardCore.OpenId.ViewModels
{
    public class OpenIdSettingsViewModel
    {
        public bool TestingModeEnabled { get; set; }
        public TokenFormat AccessTokenFormat { get; set; }
        public string Authority { get; set; }
        public string Audiences { get; set; }
        public StoreLocation? CertificateStoreLocation { get; set; }
        public StoreName? CertificateStoreName { get; set; }
        public string CertificateThumbPrint { get; set; }
        public IEnumerable<CertificateInfo> AvailableCertificates { get; set; }
        public string SslBaseUrl { get; set; }
        public bool EnableTokenEndpoint { get; set; }
        public bool EnableAuthorizationEndpoint { get; set; }
        public bool EnableLogoutEndpoint { get; set; }
        public bool EnableUserInfoEndpoint { get; set; }
        public bool AllowPasswordFlow { get; set; }
        public bool AllowClientCredentialsFlow { get; set; }
        public bool AllowAuthorizationCodeFlow { get; set; }
        public bool AllowRefreshTokenFlow { get; set; }
        public bool AllowImplicitFlow { get; set; }
        public bool UseRollingTokens { get; set; }
    }
}
