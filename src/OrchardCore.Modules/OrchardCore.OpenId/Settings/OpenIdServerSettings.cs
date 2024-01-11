using System;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.OpenId.Settings
{
    public class OpenIdServerSettings
    {
        public TokenFormat AccessTokenFormat { get; set; }

        public Uri Authority { get; set; }

        public bool DisableAccessTokenEncryption { get; set; }
        public StoreLocation? EncryptionCertificateStoreLocation { get; set; }
        public StoreName? EncryptionCertificateStoreName { get; set; }
        public string EncryptionCertificateThumbprint { get; set; }
        public StoreLocation? SigningCertificateStoreLocation { get; set; }
        public StoreName? SigningCertificateStoreName { get; set; }
        public string SigningCertificateThumbprint { get; set; }
        public PathString AuthorizationEndpointPath { get; set; }

        public PathString LogoutEndpointPath { get; set; }

        public PathString TokenEndpointPath { get; set; }

        public PathString UserinfoEndpointPath { get; set; }

        public PathString IntrospectionEndpointPath { get; set; }

        public PathString RevocationEndpointPath { get; set; }

        public bool AllowPasswordFlow { get; set; }
        public bool AllowClientCredentialsFlow { get; set; }
        public bool AllowAuthorizationCodeFlow { get; set; }
        public bool AllowRefreshTokenFlow { get; set; }
        public bool AllowHybridFlow { get; set; }
        public bool AllowImplicitFlow { get; set; }

        public bool DisableRollingRefreshTokens { get; set; }

        public bool UseReferenceAccessTokens { get; set; }

        public bool RequireProofKeyForCodeExchange { get; set; }

        public enum TokenFormat
        {
            DataProtection = 0,
            JsonWebToken = 1
        }
    }
}
