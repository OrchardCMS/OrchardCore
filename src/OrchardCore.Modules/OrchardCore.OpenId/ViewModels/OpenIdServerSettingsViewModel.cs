using System.Security.Cryptography.X509Certificates;
using static OrchardCore.OpenId.Settings.OpenIdServerSettings;

namespace OrchardCore.OpenId.ViewModels;

public class OpenIdServerSettingsViewModel
{
    public TokenFormat AccessTokenFormat { get; set; }

    [Url]
    public string Authority { get; set; }
    public bool DisableAccessTokenEncryption { get; set; }
    public StoreLocation? EncryptionCertificateStoreLocation { get; set; }
    public StoreName? EncryptionCertificateStoreName { get; set; }
    public string EncryptionCertificateThumbprint { get; set; }
    public StoreLocation? SigningCertificateStoreLocation { get; set; }
    public StoreName? SigningCertificateStoreName { get; set; }
    public string SigningCertificateThumbprint { get; set; }
    public IList<CertificateInfo> AvailableCertificates { get; } = [];
    public bool EnableTokenEndpoint { get; set; }
    public bool EnableAuthorizationEndpoint { get; set; }
    public bool EnableLogoutEndpoint { get; set; }
    public bool EnableUserInfoEndpoint { get; set; }
    public bool EnableIntrospectionEndpoint { get; set; }
    public bool EnableRevocationEndpoint { get; set; }
    public bool AllowPasswordFlow { get; set; }
    public bool AllowClientCredentialsFlow { get; set; }
    public bool AllowAuthorizationCodeFlow { get; set; }
    public bool AllowRefreshTokenFlow { get; set; }
    public bool AllowHybridFlow { get; set; }
    public bool AllowImplicitFlow { get; set; }
    public bool DisableRollingRefreshTokens { get; set; }
    public bool UseReferenceAccessTokens { get; set; }
    public bool RequireProofKeyForCodeExchange { get; set; }

    public class CertificateInfo
    {
        public string FriendlyName { get; set; }
        public string Issuer { get; set; }
        public DateTime NotAfter { get; set; }
        public DateTime NotBefore { get; set; }
        public StoreLocation StoreLocation { get; set; }
        public StoreName StoreName { get; set; }
        public string Subject { get; set; }
        public string ThumbPrint { get; set; }
        public bool HasPrivateKey { get; set; }
        public bool Archived { get; set; }
    }
}
