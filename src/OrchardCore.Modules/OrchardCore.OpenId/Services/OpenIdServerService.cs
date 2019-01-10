using System;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using OrchardCore.OpenId.Settings;
using OrchardCore.Settings;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace OrchardCore.OpenId.Services
{
    public class OpenIdServerService : IOpenIdServerService
    {
        private readonly IDataProtector _dataProtector;
        private readonly ISiteService _siteService;
        private readonly IStringLocalizer<OpenIdServerService> T;

        public OpenIdServerService(
            IDataProtectionProvider dataProtectionProvider,
            ISiteService siteService,
            IStringLocalizer<OpenIdServerService> stringLocalizer)
        {
            _dataProtector = dataProtectionProvider.CreateProtector(nameof(OpenIdServerService));
            _siteService = siteService;
            T = stringLocalizer;
        }

        public async Task<OpenIdServerSettings> GetSettingsAsync()
        {
            var container = await _siteService.GetSiteSettingsAsync();
            if (container.Properties.TryGetValue(nameof(OpenIdServerSettings), out var settings))
            {
                return settings.ToObject<OpenIdServerSettings>();
            }

            // If the OpenID server settings haven't been populated yet, the authorization,
            // logout, token and userinfo endpoints are assumed to be enabled by default.
            // In this case, only the authorization code and refresh token flows are used.
            return new OpenIdServerSettings
            {
                AuthorizationEndpointPath = "/connect/authorize",
                GrantTypes = { GrantTypes.AuthorizationCode, GrantTypes.RefreshToken },
                LogoutEndpointPath = "/connect/logout",
                TokenEndpointPath = "/connect/token",
                UserinfoEndpointPath = "/connect/userinfo"
            };
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
            }

            if (settings.CertificateStoreLocation != null &&
                settings.CertificateStoreName != null &&
                !string.IsNullOrEmpty(settings.CertificateThumbprint))
            {
                var certificate = GetCertificate(
                    settings.CertificateStoreLocation.Value,
                    settings.CertificateStoreName.Value, settings.CertificateThumbprint);

                if (certificate == null)
                {
                    results.Add(new ValidationResult(T["The certificate cannot be found."], new[]
                    {
                        nameof(settings.CertificateThumbprint)
                    }));
                }

                else if (!certificate.HasPrivateKey)
                {
                    results.Add(new ValidationResult(T["The certificate doesn't contain the required private key."], new[]
                    {
                        nameof(settings.CertificateThumbprint)
                    }));
                }

                else if (certificate.Archived)
                {
                    results.Add(new ValidationResult(T["The certificate is not valid because it is marked as archived."], new[]
                    {
                        nameof(settings.CertificateThumbprint)
                    }));
                }

                else if (certificate.NotBefore > DateTime.Now || certificate.NotAfter < DateTime.Now)
                {
                    results.Add(new ValidationResult(T["The certificate is not valid for current date."], new[]
                    {
                        nameof(settings.CertificateThumbprint)
                    }));
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
            var keys = ImmutableArray.CreateBuilder<SecurityKey>();

            var settings = await GetSettingsAsync();

            // If a certificate was provided, register it before adding the signing keys.
            if (settings.CertificateStoreLocation != null &&
                settings.CertificateStoreName != null &&
                !string.IsNullOrEmpty(settings.CertificateThumbprint))
            {
                var certificate = GetCertificate(
                    settings.CertificateStoreLocation.Value,
                    settings.CertificateStoreName.Value, settings.CertificateThumbprint);

                if (certificate != null)
                {
                    keys.Add(new X509SecurityKey(certificate));
                }
            }

            foreach (var key in settings.SigningKeys.OrderByDescending(key => key.ExpirationDate).ToList())
            {
                // If the signing key has an expiration date, only add it if is still valid.
                if (key.ExpirationDate != null && key.ExpirationDate.Value.AddDays(1) < DateTimeOffset.UtcNow)
                {
                    continue;
                }

                // Note: only RSA signing keys are currently supported by the OpenID module.
                if (!string.Equals(key.Type, JsonWebAlgorithmsKeyTypes.RSA, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // Note: the parameters may be null if the payload couldn't be decrypted.
                var parameters = DecryptRsaSigningKey(key.Payload);
                if (parameters == null)
                {
                    continue;
                }

                keys.Add(new RsaSecurityKey(parameters.Value));
            }

            return keys.ToImmutable();

            RSAParameters? DecryptRsaSigningKey(ReadOnlySpan<byte> payload)
            {
                // Note: an exception thrown in this block may be caused by a corrupted or inappropriate key
                // (for instance, if the key was created for another application or another environment).
                // Always catch the exception and return null in this case to avoid leaking sensitive data.
                try
                {
                    var bytes = _dataProtector.Unprotect(payload.ToArray());
                    if (bytes == null)
                    {
                        return null;
                    }

                    using (var stream = new MemoryStream(bytes))
                    using (var reader = new BinaryReader(stream))
                    {
                        return new RSAParameters
                        {
                            D = reader.ReadBytes(reader.ReadInt32()),
                            DP = reader.ReadBytes(reader.ReadInt32()),
                            DQ = reader.ReadBytes(reader.ReadInt32()),
                            Exponent = reader.ReadBytes(reader.ReadInt32()),
                            InverseQ = reader.ReadBytes(reader.ReadInt32()),
                            Modulus = reader.ReadBytes(reader.ReadInt32()),
                            P = reader.ReadBytes(reader.ReadInt32()),
                            Q = reader.ReadBytes(reader.ReadInt32())
                        };
                    }
                }

                catch
                {
                    return null;
                }
            }
        }

        public async Task AddSigningKeyAsync(SecurityKey key)
        {
            var settings = await GetSettingsAsync();

            if (key is RsaSecurityKey rsaSecurityKey)
            {
                settings.SigningKeys.Add(new OpenIdServerSettings.SigningKey
                {
                    CreationDate = DateTimeOffset.UtcNow,
                    ExpirationDate = DateTimeOffset.UtcNow.AddDays(90),
                    Payload = EncryptRsaSigningKey(rsaSecurityKey.Rsa).ToArray(),
                    Type = JsonWebAlgorithmsKeyTypes.RSA
                });
            }
            else
            {
                throw new InvalidOperationException("The specified security key is not supported.");
            }

            await UpdateSettingsAsync(settings);

            ReadOnlySpan<byte> EncryptRsaSigningKey(RSA algorithm)
            {
                var parameters = algorithm.ExportParameters(includePrivateParameters: true);

                using (var stream = new MemoryStream())
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(parameters.D.Length);
                    writer.Write(parameters.D);
                    writer.Write(parameters.DP.Length);
                    writer.Write(parameters.DP);
                    writer.Write(parameters.DQ.Length);
                    writer.Write(parameters.DQ);
                    writer.Write(parameters.Exponent.Length);
                    writer.Write(parameters.Exponent);
                    writer.Write(parameters.InverseQ.Length);
                    writer.Write(parameters.InverseQ);
                    writer.Write(parameters.Modulus.Length);
                    writer.Write(parameters.Modulus);
                    writer.Write(parameters.P.Length);
                    writer.Write(parameters.P);
                    writer.Write(parameters.Q.Length);
                    writer.Write(parameters.Q);
                    writer.Flush();

                    return _dataProtector.Protect(stream.ToArray());
                }
            }
        }

        public Task<SecurityKey> GenerateSigningKeyAsync() => GenerateSigningKeyAsync<RsaSecurityKey>();

        public Task<SecurityKey> GenerateSigningKeyAsync<TSecurityKey>() where TSecurityKey : SecurityKey
        {
            if (typeof(TSecurityKey) == typeof(RsaSecurityKey))
            {
                return Task.FromResult<SecurityKey>(new RsaSecurityKey(GenerateRsaSigningKey(size: 2048)));
            }

            throw new InvalidOperationException("The specified security key type is not supported.");

            RSA GenerateRsaSigningKey(int size)
            {
                RSA algorithm = null;

                // By default, the default RSA implementation used by .NET Core relies on the newest Windows CNG APIs.
                // Unfortunately, when a new key is generated using the default RSA.Create() method, it is not bound
                // to the machine account, which may cause security exceptions when running Orchard on IIS using a
                // virtual application pool identity or without the profile loading feature enabled (off by default).
                // To ensure a RSA key can be generated flawlessly, it is manually created using the managed CNG APIs.
                // For more information, visit https://github.com/openiddict/openiddict-core/issues/204.
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // Warning: ensure a null key name is specified to ensure the RSA key is not persisted by CNG.
                    var key = CngKey.Create(CngAlgorithm.Rsa, keyName: null, new CngKeyCreationParameters
                    {
                        ExportPolicy = CngExportPolicies.AllowPlaintextExport,
                        KeyCreationOptions = CngKeyCreationOptions.MachineKey,
                        KeyUsage = CngKeyUsages.Signing,
                        Parameters = { new CngProperty("Length", BitConverter.GetBytes(size), CngPropertyOptions.None) }
                    });

                    algorithm = new RSACng(key);
                }

                else
                {
                    algorithm = RSA.Create();

                    // Note: a 1024-bit key might be returned by RSA.Create() on .NET Desktop/Mono,
                    // where RSACryptoServiceProvider is still the default implementation and
                    // where custom implementations can be registered via CryptoConfig.
                    // To ensure the key size is always acceptable, replace it if necessary.
                    if (algorithm.KeySize < size)
                    {
                        algorithm.KeySize = size;
                    }

                    if (algorithm.KeySize < size && algorithm is RSACryptoServiceProvider)
                    {
                        algorithm.Dispose();
                        algorithm = new RSACryptoServiceProvider(size);
                    }

                    if (algorithm.KeySize < size)
                    {
                        throw new InvalidOperationException("The RSA key generation failed.");
                    }
                }

                return algorithm;
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
                    case 0: return null;
                    case 1: return certificates[0];
                    default: throw new InvalidOperationException("Multiple certificates with the same thumbprint were found.");
                }
            }
        }
    }
}
