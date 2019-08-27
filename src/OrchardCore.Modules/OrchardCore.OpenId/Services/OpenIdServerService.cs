using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using OrchardCore.Environment.Shell;
using OrchardCore.OpenId.Settings;
using OrchardCore.Settings;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace OrchardCore.OpenId.Services
{
    public class OpenIdServerService : IOpenIdServerService
    {
        private readonly IDataProtector _dataProtector;
        private readonly ILogger<OpenIdServerService> _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly IOptionsMonitor<ShellOptions> _shellOptions;
        private readonly ShellSettings _shellSettings;
        private readonly ISiteService _siteService;
        private readonly IStringLocalizer<OpenIdServerService> T;

        public OpenIdServerService(
            IDataProtectionProvider dataProtectionProvider,
            ILogger<OpenIdServerService> logger,
            IMemoryCache memoryCache,
            IOptionsMonitor<ShellOptions> shellOptions,
            ShellSettings shellSettings,
            ISiteService siteService,
            IStringLocalizer<OpenIdServerService> stringLocalizer)
        {
            _dataProtector = dataProtectionProvider.CreateProtector(nameof(OpenIdServerService));
            _logger = logger;
            _memoryCache = memoryCache;
            _shellOptions = shellOptions;
            _shellSettings = shellSettings;
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

            if (settings.Authority != null)
            {
                if (!settings.Authority.IsAbsoluteUri || !settings.Authority.IsWellFormedOriginalString())
                {
                    results.Add(new ValidationResult(T["The authority must be a valid absolute URL."], new[]
                    {
                        nameof(settings.Authority)
                    }));
                }

                if (!string.IsNullOrEmpty(settings.Authority.Query) || !string.IsNullOrEmpty(settings.Authority.Fragment))
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
            var settings = await GetSettingsAsync();

            // If a certificate was explicitly provided, return it immediately
            // instead of using the fallback managed certificates logic.
            if (settings.CertificateStoreLocation != null &&
                settings.CertificateStoreName != null &&
                !string.IsNullOrEmpty(settings.CertificateThumbprint))
            {
                var certificate = GetCertificate(
                    settings.CertificateStoreLocation.Value,
                    settings.CertificateStoreName.Value, settings.CertificateThumbprint);

                if (certificate != null)
                {
                    return ImmutableArray.Create<SecurityKey>(new X509SecurityKey(certificate));
                }

                _logger.LogWarning("The signing certificate '{Thumbprint}' could not be found in the " +
                                   "{StoreLocation}/{StoreName} store.", settings.CertificateThumbprint,
                                   settings.CertificateStoreLocation.Value.ToString(),
                                   settings.CertificateStoreName.Value.ToString());
            }

            try
            {
                var certificates = (await GetManagedSigningCertificatesAsync()).Select(tuple => tuple.certificate).ToList();
                if (certificates.Any(certificate => certificate.NotAfter.AddDays(-7) > DateTime.Now))
                {
                    return ImmutableArray.CreateRange<SecurityKey>(
                        from certificate in certificates
                        select new X509SecurityKey(certificate));
                }

#if SUPPORTS_CERTIFICATE_GENERATION
                try
                {
                    // If the certificates list is empty or only contains certificates about to expire,
                    // generate a new certificate and add it on top of the list to ensure it's preferred
                    // by OpenIddict to the other certificates when issuing JWT access or identity tokens.
                    certificates.Insert(0, await GenerateSigningCertificateAsync());

                    return ImmutableArray.CreateRange<SecurityKey>(
                        from certificate in certificates
                        select new X509SecurityKey(certificate));
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "An error occurred while trying to generate a X.509 signing certificate.");
                }
#else
                _logger.LogError("This platform doesn't support X.509 certificate generation.");
#endif
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "An error occurred while trying to retrieve the X.509 signing certificates.");
            }

            // If none of the previous attempts succeeded, try to generate an ephemeral RSA key
            // and add it in the tenant memory cache so that future calls to this method return it.
            return ImmutableArray.Create<SecurityKey>(_memoryCache.GetOrCreate(nameof(RsaSecurityKey), entry =>
            {
                entry.SetPriority(CacheItemPriority.NeverRemove);

                return new RsaSecurityKey(GenerateRsaSigningKey(2048));
            }));

            RSA GenerateRsaSigningKey(int size)
            {
                RSA algorithm;

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
#if SUPPORTS_DIRECT_KEY_CREATION_WITH_SPECIFIED_SIZE
                    algorithm = RSA.Create(size);
#else
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
#endif
                }

                return algorithm;
            }

#if SUPPORTS_CERTIFICATE_GENERATION
            async Task<X509Certificate2> GenerateSigningCertificateAsync()
            {
                var subject = GetSubjectName();
                var algorithm = GenerateRsaSigningKey(size: 2048);

                // Note: ensure the digitalSignature bit is added to the certificate, so that no validation error
                // is returned to clients that fully validate the certificates chain and their X.509 key usages.
                var request = new CertificateRequest(subject, algorithm, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, critical: true));

                var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddMonths(3));

                // Note: setting the friendly name is not supported on Unix machines (including Linux and macOS).
                // To ensure an exception is not thrown by the property setter, an OS runtime check is used here.
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    certificate.FriendlyName = "OrchardCore OpenID Server Signing Certificate";
                }

                var directory = Directory.CreateDirectory(Path.Combine(
                    _shellOptions.CurrentValue.ShellsApplicationDataPath,
                    _shellOptions.CurrentValue.ShellsContainerName,
                    _shellSettings.Name, "IdentityModel-Signing-Certificates"));

                var password = GeneratePassword();
                var path = Path.Combine(directory.FullName, Guid.NewGuid().ToString());

                await File.WriteAllBytesAsync(Path.ChangeExtension(path, ".pfx"), certificate.Export(X509ContentType.Pfx, password));
                await File.WriteAllTextAsync(Path.ChangeExtension(path, ".pwd"), _dataProtector.Protect(password));

                return certificate;

                X500DistinguishedName GetSubjectName()
                {
                    try { return new X500DistinguishedName("CN=" + (_shellSettings.RequestUrlHost ?? "localhost")); }
                    catch { return new X500DistinguishedName("CN=localhost"); }
                }

                string GeneratePassword()
                {
                    Span<byte> data = stackalloc byte[256 / 8];
                    RandomNumberGenerator.Fill(data);
                    return Convert.ToBase64String(data, Base64FormattingOptions.None);
                }
            }
#endif
        }

        public async Task PruneSigningKeysAsync()
        {
            List<Exception> exceptions = null;

            foreach (var (path, certificate) in await GetManagedSigningCertificatesAsync())
            {
                // Only delete expired certificates that expired at least 7 days ago.
                if (certificate.NotAfter.AddDays(7) < DateTime.Now)
                {
                    try
                    {
                        // Delete both the X.509 certificate and its password file.
                        File.Delete(path);
                        File.Delete(Path.ChangeExtension(path, ".pwd"));
                    }
                    catch (Exception exception)
                    {
                        if (exceptions == null)
                        {
                            exceptions = new List<Exception>();
                        }

                        exceptions.Add(exception);
                    }
                }
            }

            if (exceptions != null)
            {
                throw new AggregateException(exceptions);
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

        private async Task<ImmutableArray<(string path, X509Certificate2 certificate)>> GetManagedSigningCertificatesAsync()
        {
            var directory = new DirectoryInfo(Path.Combine(
                _shellOptions.CurrentValue.ShellsApplicationDataPath,
                _shellOptions.CurrentValue.ShellsContainerName,
                _shellSettings.Name, "IdentityModel-Signing-Certificates"));

            if (!directory.Exists)
            {
                return ImmutableArray.Create<(string, X509Certificate2)>();
            }

            var certificates = ImmutableArray.CreateBuilder<(string, X509Certificate2)>();

            foreach (var file in directory.EnumerateFiles("*.pfx", SearchOption.TopDirectoryOnly))
            {
                try
                {
                    // Only add the certificate if it's still valid.
                    var certificate = await GetCertificateAsync(file.FullName);
                    if (certificate.NotBefore <= DateTime.Now && certificate.NotAfter > DateTime.Now)
                    {
                        certificates.Add((file.FullName, certificate));
                    }
                }
                catch (Exception exception)
                {
                    _logger.LogWarning(exception, "An error occurred while trying to extract a X.509 certificate.");

                    continue;
                }
            }

            return certificates.ToImmutable();

            async Task<string> GetPasswordAsync(string path)
            {
                using (var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var reader = new StreamReader(stream))
                {
                    return _dataProtector.Unprotect(await reader.ReadToEndAsync());
                }
            }

            async Task<X509Certificate2> GetCertificateAsync(string path)
            {
                // Extract the certificate password from the separate .pwd file.
                var password = await GetPasswordAsync(Path.ChangeExtension(path, ".pwd"));

                try
                {
                    // Note: ephemeral key sets are not supported on non-Windows platforms.
                    var flags =
#if SUPPORTS_EPHEMERAL_KEY_SETS
                        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
                            X509KeyStorageFlags.EphemeralKeySet :
                            X509KeyStorageFlags.MachineKeySet;
#else
                            X509KeyStorageFlags.MachineKeySet;
#endif

                    return new X509Certificate2(path, password, flags);
                }
                // Some cloud platforms (e.g Azure App Service/Antares) are known to fail to import .pfx files if the
                // private key is not persisted or marked as exportable. To ensure X.509 certificates can be correctly
                // read on these platforms, a second pass is made by specifying the PersistKeySet and Exportable flags.
                // For more information, visit https://github.com/OrchardCMS/OrchardCore/issues/3222.
                catch (CryptographicException exception)
                {
                    _logger.LogDebug(exception, "A first-chance exception occurred while trying to extract " +
                                                "a X.509 certificate with the default key storage options.");

                    return new X509Certificate2(path, password,
                        X509KeyStorageFlags.MachineKeySet |
                        X509KeyStorageFlags.PersistKeySet |
                        X509KeyStorageFlags.Exportable);
                }
                // Don't swallow exceptions thrown from the catch handler to ensure unrecoverable exceptions
                // (e.g caused by malformed X.509 certificates or invalid password) are correctly logged.
            }
        }
    }
}
