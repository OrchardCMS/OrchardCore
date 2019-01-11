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
        private readonly IOptionsMonitor<ShellOptions> _shellOptions;
        private readonly ShellSettings _shellSettings;
        private readonly ISiteService _siteService;
        private readonly IStringLocalizer<OpenIdServerService> T;

        public OpenIdServerService(
            IDataProtectionProvider dataProtectionProvider,
            ILogger<OpenIdServerService> logger,
            IOptionsMonitor<ShellOptions> shellOptions,
            ShellSettings shellSettings,
            ISiteService siteService,
            IStringLocalizer<OpenIdServerService> stringLocalizer)
        {
            _dataProtector = dataProtectionProvider.CreateProtector(nameof(OpenIdServerService));
            _logger = logger;
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
                var certificates = await GetCertificatesAsync();
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
                    certificates = certificates.Insert(0, await GenerateSigningCertificateAsync());

                    return ImmutableArray.CreateRange<SecurityKey>(
                        from certificate in certificates
                        select new X509SecurityKey(certificate));
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "An error occured while trying to generate a X.509 signing certificate.");
                }
#else
                _logger.LogError("This platform doesn't support X.509 certificate generation.");
#endif
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "An error occurred while trying to retrieve the X.509 signing certificates.");
            }

            // If none of the previous attempts succeeded, try to generate an ephemeral RSA key.
            return ImmutableArray.Create<SecurityKey>(new RsaSecurityKey(GenerateRsaSigningKey(2048)));

            async Task<ImmutableArray<X509Certificate2>> GetCertificatesAsync()
            {
                var directory = new DirectoryInfo(Path.Combine(
                    _shellOptions.CurrentValue.ShellsApplicationDataPath,
                    _shellOptions.CurrentValue.ShellsContainerName,
                    _shellSettings.Name, "IdentityModel-Signing-Certificates"));

                if (!directory.Exists)
                {
                    return ImmutableArray.Create<X509Certificate2>();
                }

                var certificates = ImmutableArray.CreateBuilder<X509Certificate2>();

                foreach (var file in directory.EnumerateFiles("*.pfx", SearchOption.TopDirectoryOnly))
                {
                    try
                    {
                        // Extract the certificate password from the separate .pwd file.
                        var password = await GetPasswordAsync(Path.ChangeExtension(file.FullName, ".pwd"));

                        var flags =
#if SUPPORTS_EPHEMERAL_KEY_SETS
                            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
                                X509KeyStorageFlags.EphemeralKeySet :
                                X509KeyStorageFlags.MachineKeySet;
#else
                                X509KeyStorageFlags.MachineKeySet;
#endif

                        // Only add the certificate if it's still valid.
                        var certificate = new X509Certificate2(file.FullName, password, flags);
                        if (certificate.NotBefore <= DateTime.Now && certificate.NotAfter > DateTime.Now)
                        {
                            certificates.Add(certificate);
                        }
                    }
                    catch (Exception exception)
                    {
                        _logger.LogWarning(exception, "An error occurred while trying to extract a X.509 certificate.");

                        continue;
                    }
                }

                return certificates.ToImmutable();
            }

            async Task<string> GetPasswordAsync(string path)
            {
                using (var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var reader = new StreamReader(stream))
                {
                    return _dataProtector.Unprotect(await reader.ReadToEndAsync());
                }
            }

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

                return await SaveSigningCertificateAsync(certificate, GeneratePassword());
            }

            X500DistinguishedName GetSubjectName()
            {
                try { return new X500DistinguishedName("CN=" + (_shellSettings.RequestUrlHost ?? "localhost")); }
                catch { return new X500DistinguishedName("CN=localhost"); }
            }

            string GeneratePassword()
            {
                Span<byte> password = stackalloc byte[256 / 8];
                RandomNumberGenerator.Fill(password);
                return Convert.ToBase64String(password, Base64FormattingOptions.None);
            }

            async Task<X509Certificate2> SaveSigningCertificateAsync(X509Certificate2 certificate, string password)
            {
                var directory = Directory.CreateDirectory(Path.Combine(
                    _shellOptions.CurrentValue.ShellsApplicationDataPath,
                    _shellOptions.CurrentValue.ShellsContainerName,
                    _shellSettings.Name, "IdentityModel-Signing-Certificates"));

                var path = Path.Combine(directory.FullName, Guid.NewGuid().ToString());
                await File.WriteAllBytesAsync(Path.ChangeExtension(path, ".pfx"), certificate.Export(X509ContentType.Pfx, password));
                await File.WriteAllTextAsync(Path.ChangeExtension(path, ".pwd"), _dataProtector.Protect(password));

                return certificate;
            }
#endif
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
