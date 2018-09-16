using System;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using OrchardCore.Entities;
using OrchardCore.OpenId.Settings;
using OrchardCore.Settings;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace OrchardCore.OpenId.Services
{
    public class OpenIdServerService : IOpenIdServerService
    {
        private readonly IMemoryCache _cache;
        private readonly ISiteService _siteService;
        private readonly IStringLocalizer<OpenIdServerService> T;

        public OpenIdServerService(
            IMemoryCache cache,
            ISiteService siteService,
            IStringLocalizer<OpenIdServerService> stringLocalizer)
        {
            _cache = cache;
            _siteService = siteService;
            T = stringLocalizer;
        }

        public async Task<OpenIdServerSettings> GetSettingsAsync()
        {
            var container = await _siteService.GetSiteSettingsAsync();
            return container.As<OpenIdServerSettings>();
        }

        public async Task UpdateSettingsAsync(OpenIdServerSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var container = await _siteService.GetSiteSettingsAsync();
            container.Properties[nameof(OpenIdServerSettings)] = JObject.FromObject(settings);
            await _siteService.UpdateSiteSettingsAsync(container);
        }

        public Task<ImmutableArray<ValidationResult>> ValidateSettingsAsync(OpenIdServerSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var results = ImmutableArray.CreateBuilder<ValidationResult>();

            if (settings.GrantTypes.Count == 0)
            {
                results.Add(new ValidationResult(T["At least one OpenID Connect flow must be enabled."]));
            }

            if (!string.IsNullOrEmpty(settings.Authority))
            {
                if (!Uri.TryCreate(settings.Authority, UriKind.Absolute, out Uri uri) || !uri.IsWellFormedOriginalString())
                {
                    results.Add(new ValidationResult(T["The authority must be a valid absolute URL."], new[]
                    {
                        nameof(settings.Authority)
                    }));
                }

                if (!string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment))
                {
                    results.Add(new ValidationResult(T["The authority cannot contain a query string or a fragment."], new[]
                    {
                        nameof(settings.Authority)
                    }));
                }

                if (!settings.TestingModeEnabled && string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(new ValidationResult(T["The authority cannot be a HTTP URL when the testing mode is disabled."], new[]
                    {
                        nameof(settings.Authority)
                    }));
                }
            }

            if (!settings.TestingModeEnabled)
            {
                if (settings.CertificateStoreName == null)
                {
                    results.Add(new ValidationResult(T["A Certificate Store Name is required."], new[]
                    {
                        nameof(settings.CertificateStoreName)
                    }));
                }

                if (settings.CertificateStoreLocation == null)
                {
                    results.Add(new ValidationResult(T["A Certificate Store Location is required."], new[]
                    {
                        nameof(settings.CertificateStoreLocation)
                    }));
                }

                if (string.IsNullOrEmpty(settings.CertificateThumbprint))
                {
                    results.Add(new ValidationResult(T["A certificate is required when testing mode is disabled."], new[]
                    {
                        nameof(settings.CertificateThumbprint)
                    }));
                }

                if (settings.CertificateStoreLocation != null &&
                    settings.CertificateStoreName != null &&
                    !string.IsNullOrEmpty(settings.CertificateThumbprint))
                {
                    var certificate = GetCertificate(settings.CertificateStoreLocation.Value,
                        settings.CertificateStoreName.Value, settings.CertificateThumbprint);

                    if (certificate == null)
                    {
                        results.Add(new ValidationResult(T["The certificate cannot be found."], new[]
                        {
                            nameof(settings.CertificateThumbprint)
                        }));
                    }

                    if (!certificate.HasPrivateKey)
                    {
                        results.Add(new ValidationResult(T["The certificate doesn't contain the required private key."], new[]
                        {
                            nameof(settings.CertificateThumbprint)
                        }));
                    }

                    if (certificate.Archived)
                    {
                        results.Add(new ValidationResult(T["The certificate is not valid because it is marked as archived."], new[]
                        {
                            nameof(settings.CertificateThumbprint)
                        }));
                    }

                    if (certificate.NotBefore > DateTime.Now || certificate.NotAfter < DateTime.Now)
                    {
                        results.Add(new ValidationResult(T["The certificate is not valid for current date."], new[]
                        {
                            nameof(settings.CertificateThumbprint)
                        }));
                    }
                }
            }

            if (settings.GrantTypes.Contains(GrantTypes.Password) && !settings.TokenEndpointPath.HasValue)
            {
                results.Add(new ValidationResult(T["The password flow cannot be enabled when the token endpoint is disabled."], new[]
                {
                    nameof(settings.GrantTypes)
                }));
            }

            if (settings.GrantTypes.Contains(GrantTypes.ClientCredentials) && !settings.TokenEndpointPath.HasValue)
            {
                results.Add(new ValidationResult(T["The client credentials flow cannot be enabled when the token endpoint is disabled."], new[]
                {
                    nameof(settings.GrantTypes)
                }));
            }

            if (settings.GrantTypes.Contains(GrantTypes.AuthorizationCode) &&
               (!settings.AuthorizationEndpointPath.HasValue || !settings.TokenEndpointPath.HasValue))
            {
                results.Add(new ValidationResult(T["The authorization code flow cannot be enabled when the authorization and token endpoints are disabled."], new[]
                {
                    nameof(settings.GrantTypes)
                }));
            }

            if (settings.GrantTypes.Contains(GrantTypes.RefreshToken))
            {
                if (!settings.TokenEndpointPath.HasValue)
                {
                    results.Add(new ValidationResult(T["The refresh token flow cannot be enabled when the token endpoint is disabled."], new[]
                    {
                        nameof(settings.GrantTypes)
                    }));
                }

                if (!settings.GrantTypes.Contains(GrantTypes.Password) && !settings.GrantTypes.Contains(GrantTypes.AuthorizationCode))
                {
                    results.Add(new ValidationResult(T["The refresh token flow can only be enabled if the password, authorization code or hybrid flows are enabled."], new[]
                    {
                        nameof(settings.GrantTypes)
                    }));
                }
            }

            if (settings.GrantTypes.Contains(GrantTypes.Implicit) && !settings.AuthorizationEndpointPath.HasValue)
            {
                results.Add(new ValidationResult(T["The implicit flow cannot be enabled when the authorization endpoint is disabled."], new[]
                {
                    nameof(settings.GrantTypes)
                }));
            }

            return Task.FromResult(results.ToImmutable());
        }

        public Task<ImmutableArray<(X509Certificate2 certificate, StoreLocation location, StoreName name)>> GetAvailableCertificatesAsync()
        {
            var certificates = ImmutableArray.CreateBuilder<(X509Certificate2, StoreLocation, StoreName)>();

            foreach (StoreLocation location in Enum.GetValues(typeof(StoreLocation)))
            {
                foreach (StoreName name in Enum.GetValues(typeof(StoreName)))
                {
                    // Note: on non-Windows platforms, an exception can
                    // be thrown if the store location/name doesn't exist.
                    try
                    {
                        using (var store = new X509Store(name, location))
                        {
                            store.Open(OpenFlags.ReadOnly);

                            foreach (var certificate in store.Certificates)
                            {
                                if (!certificate.Archived && certificate.HasPrivateKey)
                                {
                                    certificates.Add((certificate, location, name));
                                }
                            }
                        }
                    }
                    catch (CryptographicException)
                    {
                        continue;
                    }
                }
            }

            return Task.FromResult(certificates.ToImmutable());
        }

        public async Task<ImmutableArray<SecurityKey>> GetSigningKeysAsync()
        {
            var settings = await GetSettingsAsync();
            if (settings.TestingModeEnabled)
            {
                var parameters = _cache.Get(nameof(RSAParameters)) as RSAParameters?;
                if (parameters == null)
                {
                    parameters = _cache.Set(nameof(RSAParameters), GenerateRsaKey(2048));
                }

                var algorithm = RSA.Create();
                algorithm.ImportParameters(parameters.Value);

                return ImmutableArray.Create<SecurityKey>(new RsaSecurityKey(algorithm));
            }

            if (settings.CertificateStoreLocation != null &&
                settings.CertificateStoreName != null &&
                !string.IsNullOrEmpty(settings.CertificateThumbprint))
            {
                var certificate = GetCertificate(
                    settings.CertificateStoreLocation.Value,
                    settings.CertificateStoreName.Value, settings.CertificateThumbprint);

                return ImmutableArray.Create<SecurityKey>(new X509SecurityKey(certificate));
            }

            throw new InvalidOperationException("No appropriate signing key was found.");

            RSAParameters GenerateRsaKey(int size)
            {
                // Note: a 1024-bit key might be returned by RSA.Create() on .NET Desktop/Mono,
                // where RSACryptoServiceProvider is still the default implementation and
                // where custom implementations can be registered via CryptoConfig.
                // To ensure the key size is always acceptable, replace it if necessary.
                var rsa = RSA.Create();

                try
                {
                    if (rsa.KeySize < size)
                    {
                        rsa.KeySize = size;
                    }

                    if (rsa.KeySize < size && rsa is RSACryptoServiceProvider)
                    {
                        rsa.Dispose();
                        rsa = new RSACryptoServiceProvider(size);
                    }

                    if (rsa.KeySize < size)
                    {
                        throw new InvalidOperationException("The RSA key generation failed.");
                    }

                    return rsa.ExportParameters(includePrivateParameters: true);
                }

                finally
                {
                    rsa?.Dispose();
                }
            }
        }

        private static X509Certificate2 GetCertificate(StoreLocation location, StoreName name, string thumbprint)
        {
            using (var store = new X509Store(name, location))
            {
                store.Open(OpenFlags.ReadOnly);

                var certificates = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, validOnly: false);

                switch (certificates.Count)
                {
                    case 0: throw new InvalidOperationException("The certificate was not found.");
                    case 1: return certificates[0];
                    default: throw new InvalidOperationException("Multiple certificates with the same thumbprint were found.");
                }
            }
        }
    }
}
