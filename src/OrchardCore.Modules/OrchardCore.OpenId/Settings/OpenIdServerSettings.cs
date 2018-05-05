using System.Security.Cryptography.X509Certificates;

namespace OrchardCore.OpenId.Settings
{
    public class OpenIdServerSettings
    {
        public bool TestingModeEnabled { get; set; }
        public TokenFormat AccessTokenFormat { get; set; }
        public string Authority { get; set; }
        public StoreLocation? CertificateStoreLocation { get; set; }
        public StoreName? CertificateStoreName { get; set; }
        public string CertificateThumbprint { get; set; }
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

        public enum TokenFormat
        {
            Encrypted = 0,
            JWT = 1
        }
    }
}
