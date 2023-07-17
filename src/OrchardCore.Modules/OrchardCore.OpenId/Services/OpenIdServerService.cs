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

namespace OrchardCore.OpenId.Services
{
    public class OpenIdServerService : IOpenIdServerService
    {
        private readonly IDataProtector _dataProtector;
        private readonly ILogger _logger;
        private readonly IMemoryCache _memoryCache;
        private readonly IOptionsMonitor<ShellOptions> _shellOptions;
        private readonly ShellSettings _shellSettings;
        private readonly ISiteService _siteService;
        protected readonly IStringLocalizer S;

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
            S = stringLocalizer;
        }

        public async Task<OpenIdServerSettings> GetSettingsAsync()
        {
            var container = await _siteService.GetSiteSettingsAsync();
            return GetSettingsFromContainer(container);
        }

        public async Task<OpenIdServerSettings> LoadSettingsAsync()
        {
            var container = await _siteService.LoadSiteSettingsAsync();
            return GetSettingsFromContainer(container);
        }

        private OpenIdServerSettings GetSettingsFromContainer(ISite container)
        {
            if (container.Properties.TryGetValue(nameof(OpenIdServerSettings), out var settings))
            {
                return settings.ToObject<OpenIdServerSettings>();
            }

            // If the OpenID server settings haven't been populated yet, the authorization,
            // logout, token, userinfo, introspection and revocation endpoints are assumed to be enabled by default.
            // In this case, only the authorization code and refresh token flows are used.
            return new OpenIdServerSettings
            {
                AllowAuthorizationCodeFlow = true,
                AllowRefreshTokenFlow = true,
                AuthorizationEndpointPath = "/connect/authorize",
                LogoutEndpointPath = "/connect/logout",
                TokenEndpointPath = "/connect/token",
                UserinfoEndpointPath = "/connect/userinfo",
                IntrospectionEndpointPath = "/connect/introspect",
                RevocationEndpointPath = "/connect/revoke"
            };
        }

        public async Task UpdateSettingsAsync(OpenIdServerSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var container = await _siteService.LoadSiteSettingsAsync();
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

            if (!settings.AllowAuthorizationCodeFlow && !settings.AllowClientCredentialsFlow &&
                !settings.AllowHybridFlow && !settings.AllowImplicitFlow &&
                !settings.AllowPasswordFlow && !settings.AllowRefreshTokenFlow)
            {
                results.Add(new ValidationResult(S["At least one OpenID Connect flow must be enabled."]));
            }

            if (settings.Authority != null)
            {
                if (!settings.Authority.IsAbsoluteUri || !settings.Authority.IsWellFormedOriginalString())
                {
                    results.Add(new ValidationResult(S["The authority must be a valid absolute URL."], new[]
                    {
                        nameof(settings.Authority)
                    }));
                }

                if (!String.IsNullOrEmpty(settings.Authority.Query) || !String.IsNullOrEmpty(settings.Authority.Fragment))
                {
                    results.Add(new ValidationResult(S["The authority cannot contain a query string or a fragment."], new[]
                    {
                        nameof(settings.Authority)
                    }));
                }
            }

            if (settings.SigningCertificateStoreLocation != null &&
                settings.SigningCertificateStoreName != null &&
                !String.IsNullOrEmpty(settings.SigningCertificateThumbprint))
            {
                var certificate = GetCertificate(
                    settings.SigningCertificateStoreLocation.Value,
                    settings.SigningCertificateStoreName.Value, settings.SigningCertificateThumbprint);

                if (certificate == null)
                {
                    results.Add(new ValidationResult(S["The certificate cannot be found."], new[]
                    {
                        nameof(settings.SigningCertificateThumbprint)
                    }));
                }
                else if (!certificate.HasPrivateKey)
                {
                    results.Add(new ValidationResult(S["The certificate doesn't contain the required private key."], new[]
                    {
                        nameof(settings.SigningCertificateThumbprint)
                    }));
                }
                else if (certificate.Archived)
                {
                    results.Add(new ValidationResult(S["The certificate is not valid because it is marked as archived."], new[]
                    {
                        nameof(settings.SigningCertificateThumbprint)
                    }));
                }
                else if (certificate.NotBefore > DateTime.Now || certificate.NotAfter < DateTime.Now)
                {
                    results.Add(new ValidationResult(S["The certificate is not valid for current date."], new[]
                    {
                        nameof(settings.SigningCertificateThumbprint)
                    }));
                }
            }

            if (settings.AllowPasswordFlow && !settings.TokenEndpointPath.HasValue)
            {
                results.Add(new ValidationResult(S["The password flow cannot be enabled when the token endpoint is disabled."], new[]
                {
                    nameof(settings.AllowPasswordFlow)
                }));
            }

            if (settings.AllowClientCredentialsFlow && !settings.TokenEndpointPath.HasValue)
            {
                results.Add(new ValidationResult(S["The client credentials flow cannot be enabled when the token endpoint is disabled."], new[]
                {
                    nameof(settings.AllowClientCredentialsFlow)
                }));
            }

            if (settings.AllowAuthorizationCodeFlow && (!settings.AuthorizationEndpointPath.HasValue || !settings.TokenEndpointPath.HasValue))
            {
                results.Add(new ValidationResult(S["The authorization code flow cannot be enabled when the authorization and token endpoints are disabled."], new[]
                {
                    nameof(settings.AllowAuthorizationCodeFlow)
                }));
            }

            if (settings.AllowRefreshTokenFlow)
            {
                if (!settings.TokenEndpointPath.HasValue)
                {
                    results.Add(new ValidationResult(S["The refresh token flow cannot be enabled when the token endpoint is disabled."], new[]
                    {
                        nameof(settings.AllowRefreshTokenFlow)
                    }));
                }

                if (!settings.AllowPasswordFlow && !settings.AllowAuthorizationCodeFlow && !settings.AllowHybridFlow)
                {
                    results.Add(new ValidationResult(S["The refresh token flow can only be enabled if the password, authorization code or hybrid flows are enabled."], new[]
                    {
                        nameof(settings.AllowRefreshTokenFlow)
                    }));
                }
            }

            if (settings.AllowImplicitFlow && !settings.AuthorizationEndpointPath.HasValue)
            {
                results.Add(new ValidationResult(S["The implicit flow cannot be enabled when the authorization endpoint is disabled."], new[]
                {
                    nameof(settings.AllowImplicitFlow)
                }));
            }

            if (settings.AllowHybridFlow && (!settings.AuthorizationEndpointPath.HasValue || !settings.TokenEndpointPath.HasValue))
            {
                results.Add(new ValidationResult(S["The hybrid flow cannot be enabled when the authorization and token endpoints are disabled."], new[]
                {
                    nameof(settings.AllowHybridFlow)
                }));
            }

            if (settings.DisableAccessTokenEncryption && settings.AccessTokenFormat != OpenIdServerSettings.TokenFormat.JsonWebToken)
            {
                results.Add(new ValidationResult(S["Access token encryption can only be disabled when using JWT tokens."], new[]
                {
                    nameof(settings.DisableAccessTokenEncryption)
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
                        using var store = new X509Store(name, location);
                        store.Open(OpenFlags.ReadOnly);

                        foreach (var certificate in store.Certificates)
                        {
                            if (!certificate.Archived && certificate.HasPrivateKey)
                            {
                                certificates.Add((certificate, location, name));
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

        public async Task<ImmutableArray<SecurityKey>> GetEncryptionKeysAsync()
        {
            var settings = await GetSettingsAsync();

            // If a certificate was explicitly provided, return it immediately
            // instead of using the fallback managed certificates logic.
            if (settings.EncryptionCertificateStoreLocation != null &&
                settings.EncryptionCertificateStoreName != null &&
                !String.IsNullOrEmpty(settings.EncryptionCertificateThumbprint))
            {
                var certificate = GetCertificate(
                    settings.EncryptionCertificateStoreLocation.Value,
                    settings.EncryptionCertificateStoreName.Value, settings.EncryptionCertificateThumbprint);

                if (certificate != null)
                {
                    return ImmutableArray.Create<SecurityKey>(new X509SecurityKey(certificate));
                }

                _logger.LogWarning("The encryption certificate '{Thumbprint}' could not be found in the " +
                                   "{StoreLocation}/{StoreName} store.", settings.EncryptionCertificateThumbprint,
                                   settings.EncryptionCertificateStoreLocation.Value.ToString(),
                                   settings.EncryptionCertificateStoreName.Value.ToString());
            }

            try
            {
                var directory = GetEncryptionCertificateDirectory(_shellOptions.CurrentValue, _shellSettings);

                var certificates = (await GetCertificatesAsync(directory)).Select(tuple => tuple.certificate).ToList();
                if (certificates.Any(certificate => certificate.NotAfter.AddDays(-7) > DateTime.Now))
                {
                    return ImmutableArray.CreateRange<SecurityKey>(
                        from certificate in certificates
                        select new X509SecurityKey(certificate));
                }

                try
                {
                    // If the certificates list is empty or only contains certificates about to expire,
                    // generate a new certificate and add it on top of the list to ensure it's preferred
                    // by OpenIddict to the other certificates when issuing new IdentityModel tokens.
                    var certificate = GenerateEncryptionCertificate(_shellSettings);
                    await PersistCertificateAsync(directory, certificate);

                    certificates.Insert(0, certificate);

                    return certificates.Select(certificate => new X509SecurityKey(certificate)).ToImmutableArray<SecurityKey>();
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "An error occurred while trying to generate a X.509 encryption certificate.");
                }
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "An error occurred while trying to retrieve the X.509 encryption certificates.");
            }

            // If none of the previous attempts succeeded, try to generate an ephemeral RSA key
            // and add it in the tenant memory cache so that future calls to this method return it.
            return ImmutableArray.Create<SecurityKey>(_memoryCache.GetOrCreate("05A24221-8C15-4E58-A0A7-56EC3E42E783", entry =>
            {
                entry.SetPriority(CacheItemPriority.NeverRemove);

                return new RsaSecurityKey(GenerateRsaSecurityKey(size: 2048));
            }));
        }

        public async Task<ImmutableArray<SecurityKey>> GetSigningKeysAsync()
        {
            var settings = await GetSettingsAsync();

            // If a certificate was explicitly provided, return it immediately
            // instead of using the fallback managed certificates logic.
            if (settings.SigningCertificateStoreLocation != null &&
                settings.SigningCertificateStoreName != null &&
                !String.IsNullOrEmpty(settings.SigningCertificateThumbprint))
            {
                var certificate = GetCertificate(
                    settings.SigningCertificateStoreLocation.Value,
                    settings.SigningCertificateStoreName.Value, settings.SigningCertificateThumbprint);

                if (certificate != null)
                {
                    return ImmutableArray.Create<SecurityKey>(new X509SecurityKey(certificate));
                }

                _logger.LogWarning("The signing certificate '{Thumbprint}' could not be found in the " +
                                   "{StoreLocation}/{StoreName} store.", settings.SigningCertificateThumbprint,
                                   settings.SigningCertificateStoreLocation.Value.ToString(),
                                   settings.SigningCertificateStoreName.Value.ToString());
            }

            try
            {
                var directory = GetSigningCertificateDirectory(_shellOptions.CurrentValue, _shellSettings);

                var certificates = (await GetCertificatesAsync(directory)).Select(tuple => tuple.certificate).ToList();
                if (certificates.Any(certificate => certificate.NotAfter.AddDays(-7) > DateTime.Now))
                {
                    return ImmutableArray.CreateRange<SecurityKey>(
                        from certificate in certificates
                        select new X509SecurityKey(certificate));
                }

                try
                {
                    // If the certificates list is empty or only contains certificates about to expire,
                    // generate a new certificate and add it on top of the list to ensure it's preferred
                    // by OpenIddict to the other certificates when issuing new IdentityModel tokens.
                    var certificate = GenerateSigningCertificate(_shellSettings);
                    await PersistCertificateAsync(directory, certificate);

                    certificates.Insert(0, certificate);

                    return certificates.Select(certificate => new X509SecurityKey(certificate)).ToImmutableArray<SecurityKey>();
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "An error occurred while trying to generate a X.509 signing certificate.");
                }
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "An error occurred while trying to retrieve the X.509 signing certificates.");
            }

            // If none of the previous attempts succeeded, try to generate an ephemeral RSA key
            // and add it in the tenant memory cache so that future calls to this method return it.
            return ImmutableArray.Create<SecurityKey>(_memoryCache.GetOrCreate("44788774-20E3-4499-86F0-AB7CE2DF97F6", entry =>
            {
                entry.SetPriority(CacheItemPriority.NeverRemove);

                return new RsaSecurityKey(GenerateRsaSecurityKey(size: 2048));
            }));
        }

        public async Task PruneManagedCertificatesAsync()
        {
            List<Exception> exceptions = null;

            var certificates = new List<(string path, X509Certificate2 certificate)>();
            certificates.AddRange(await GetCertificatesAsync(GetEncryptionCertificateDirectory(_shellOptions.CurrentValue, _shellSettings)));
            certificates.AddRange(await GetCertificatesAsync(GetSigningCertificateDirectory(_shellOptions.CurrentValue, _shellSettings)));

            foreach (var (path, certificate) in certificates)
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
                        exceptions ??= new List<Exception>();

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
            using var store = new X509Store(name, location);
            store.Open(OpenFlags.ReadOnly);

            var certificates = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, validOnly: false);

            return certificates.Count switch
            {
                0 => null,
                1 => certificates[0],
                _ => throw new InvalidOperationException("Multiple certificates with the same thumbprint were found."),
            };
        }

        private static DirectoryInfo GetEncryptionCertificateDirectory(ShellOptions options, ShellSettings settings)
            => Directory.CreateDirectory(Path.Combine(
                options.ShellsApplicationDataPath,
                options.ShellsContainerName,
                settings.Name, "IdentityModel-Encryption-Certificates"));

        private static DirectoryInfo GetSigningCertificateDirectory(ShellOptions options, ShellSettings settings)
            => Directory.CreateDirectory(Path.Combine(
                options.ShellsApplicationDataPath,
                options.ShellsContainerName,
                settings.Name, "IdentityModel-Signing-Certificates"));

        private async Task<ImmutableArray<(string path, X509Certificate2 certificate)>> GetCertificatesAsync(DirectoryInfo directory)
        {
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
                using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                using var reader = new StreamReader(stream);

                return _dataProtector.Unprotect(await reader.ReadToEndAsync());
            }

            async Task<X509Certificate2> GetCertificateAsync(string path)
            {
                // Extract the certificate password from the separate .pwd file.
                var password = await GetPasswordAsync(Path.ChangeExtension(path, ".pwd"));

                try
                {
                    // Note: ephemeral key sets are not supported on non-Windows platforms.
                    var flags = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
                        X509KeyStorageFlags.EphemeralKeySet :
                        X509KeyStorageFlags.MachineKeySet;

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

        private static X509Certificate2 GenerateEncryptionCertificate(ShellSettings settings)
        {
            var algorithm = GenerateRsaSecurityKey(size: 2048);
            var certificate = GenerateCertificate(X509KeyUsageFlags.KeyEncipherment, algorithm, settings);

            // Note: setting the friendly name is not supported on Unix machines (including Linux and macOS).
            // To ensure an exception is not thrown by the property setter, an OS runtime check is used here.
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                certificate.FriendlyName = "OrchardCore OpenID Server Encryption Certificate";
            }

            return certificate;
        }

        private static X509Certificate2 GenerateSigningCertificate(ShellSettings settings)
        {
            var algorithm = GenerateRsaSecurityKey(size: 2048);
            var certificate = GenerateCertificate(X509KeyUsageFlags.DigitalSignature, algorithm, settings);

            // Note: setting the friendly name is not supported on Unix machines (including Linux and macOS).
            // To ensure an exception is not thrown by the property setter, an OS runtime check is used here.
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                certificate.FriendlyName = "OrchardCore OpenID Server Signing Certificate";
            }

            return certificate;
        }

        private static X509Certificate2 GenerateCertificate(X509KeyUsageFlags type, RSA algorithm, ShellSettings settings)
        {
            var subject = GetSubjectName();

            // Note: ensure the digitalSignature bit is added to the certificate, so that no validation error
            // is returned to clients that fully validate the certificates chain and their X.509 key usages.
            var request = new CertificateRequest(subject, algorithm, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            request.CertificateExtensions.Add(new X509KeyUsageExtension(type, critical: true));

            return request.CreateSelfSigned(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddMonths(3));

            X500DistinguishedName GetSubjectName()
            {
                var host = settings.RequestUrlHosts.FirstOrDefault(host => host != "localhost");
                try
                {
                    return new X500DistinguishedName("CN=" + (host ?? "localhost"));
                }
                catch
                {
                    return new X500DistinguishedName("CN=localhost");
                }
            }
        }

        private static RSA GenerateRsaSecurityKey(int size)
        {
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
                    Parameters = { new CngProperty("Length", BitConverter.GetBytes(size), CngPropertyOptions.None) }
                });

                return new RSACng(key);
            }

            return RSA.Create(size);
        }

        private async Task PersistCertificateAsync(DirectoryInfo directory, X509Certificate2 certificate)
        {
            var password = GeneratePassword();
            var path = Path.Combine(directory.FullName, Guid.NewGuid().ToString());

            await File.WriteAllBytesAsync(Path.ChangeExtension(path, ".pfx"), certificate.Export(X509ContentType.Pfx, password));
            await File.WriteAllTextAsync(Path.ChangeExtension(path, ".pwd"), _dataProtector.Protect(password));

            static string GeneratePassword()
            {
                Span<byte> data = stackalloc byte[256 / 8];
                RandomNumberGenerator.Fill(data);
                return Convert.ToBase64String(data, Base64FormattingOptions.None);
            }
        }
    }
}
