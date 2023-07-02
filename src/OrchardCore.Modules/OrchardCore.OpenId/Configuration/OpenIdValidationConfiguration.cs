using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Validation;
using OpenIddict.Validation.AspNetCore;
using OpenIddict.Validation.DataProtection;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Modules;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;
using OrchardCore.Security;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace OrchardCore.OpenId.Configuration
{
    [Feature(OpenIdConstants.Features.Validation)]
    public class OpenIdValidationConfiguration : IConfigureOptions<AuthenticationOptions>,
        IConfigureOptions<OpenIddictValidationOptions>,
        IConfigureOptions<OpenIddictValidationDataProtectionOptions>,
        IConfigureNamedOptions<ApiAuthorizationOptions>
    {
        private readonly ILogger _logger;
        private readonly IOpenIdValidationService _validationService;
        private readonly IRunningShellTable _runningShellTable;
        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;

        public OpenIdValidationConfiguration(
            ILogger<OpenIdValidationConfiguration> logger,
            IOpenIdValidationService validationService,
            IRunningShellTable runningShellTable,
            IShellHost shellHost,
            ShellSettings shellSettings)
        {
            _logger = logger;
            _validationService = validationService;
            _runningShellTable = runningShellTable;
            _shellHost = shellHost;
            _shellSettings = shellSettings;
        }

        public void Configure(AuthenticationOptions options)
        {
            var settings = GetValidationSettingsAsync().GetAwaiter().GetResult();
            if (settings == null)
            {
                return;
            }

            if (settings.Authority != null)
            {
                options.AddScheme<OpenIddictValidationAspNetCoreHandler>(
                    OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme, displayName: null);

                return;
            }

            // Note: the shell host guarantees that the OpenID server service resolved inside
            // this using block won't be disposed until the service scope itself is released.
            CreateTenantScope(settings.Tenant).UsingAsync(async scope =>
            {
                var service = scope.ServiceProvider.GetService<IOpenIdServerService>();
                if (service == null)
                {
                    return;
                }

                var configuration = await GetServerSettingsAsync(service);
                if (configuration == null)
                {
                    return;
                }

                options.AddScheme<OpenIddictValidationAspNetCoreHandler>(
                    OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme, displayName: null);
            }).GetAwaiter().GetResult();
        }

        public void Configure(OpenIddictValidationOptions options)
        {
            var settings = GetValidationSettingsAsync().GetAwaiter().GetResult();
            if (settings == null)
            {
                return;
            }

            // If the tokens are issued by an authorization server located in an Orchard tenant, retrieve the
            // authority and the signing key and register them in the token validation parameters to prevent
            // the handler from using an HTTP call to retrieve the discovery document from the other tenant.
            // Otherwise, set the authority to allow the handler to retrieve the endpoint URLs/signing keys
            // from the remote server's metadata by sending an OpenID Connect/OAuth 2.0 discovery request.

            if (settings.Authority != null)
            {
                options.Issuer = settings.Authority;
                options.ConfigurationEndpoint = settings.MetadataAddress;
                options.Audiences.Add(settings.Audience);

                // Note: OpenIddict 3.0 only accepts tokens issued with a non-empty token type (e.g "at+jwt")
                // or with the generic "JWT" type and a special "token_type" claim containing the actual type
                // for backward compatibility, which matches the recommended best practices and helps prevent
                // token substitution attacks by ensuring JWT tokens of any other type are always rejected.
                // Unfortunately, most of the OAuth 2.0/OpenID Connect servers haven't been updated to emit
                // access tokens using the "at+jwt" token type header. To ensure the validation handler can still
                // be used with these servers, an option is provided to disable the token validation logic.
                // In this case, the received tokens are assumed to be access tokens (which is the only type
                // currently used in the API validation feature), no matter what their actual "typ" header is.
                if (settings.DisableTokenTypeValidation)
                {
                    options.TokenValidationParameters.TypeValidator = (type, token, parameters)
                        => JsonWebTokenTypes.AccessToken;
                }
            }

            // Note: the shell host guarantees that the OpenID server service resolved inside
            // this using block won't be disposed until the service scope itself is released.
            CreateTenantScope(settings.Tenant).UsingAsync(async scope =>
            {
                var service = scope.ServiceProvider.GetService<IOpenIdServerService>();
                if (service == null)
                {
                    return;
                }

                var configuration = await GetServerSettingsAsync(service);
                if (configuration == null)
                {
                    return;
                }

                options.Configuration = new OpenIddictConfiguration();

                options.Issuer = configuration.Authority;

                // Import the signing keys from the OpenID server configuration.
                foreach (var key in await service.GetSigningKeysAsync())
                {
                    options.Configuration.SigningKeys.Add(key);
                }

                // Register the encryption keys used by the OpenID Connect server.
                foreach (var key in await service.GetEncryptionKeysAsync())
                {
                    options.EncryptionCredentials.Add(new EncryptingCredentials(key,
                        SecurityAlgorithms.RsaOAEP, SecurityAlgorithms.Aes256CbcHmacSha512));
                }

                // When the server is another tenant, don't allow the current tenant
                // to choose the valid audiences, as this would otherwise allow it
                // to validate/introspect tokens meant to be used with another tenant.
                options.Audiences.Add(OpenIdConstants.Prefixes.Tenant + _shellSettings.Name);

                // Note: token entry validation must be enabled to be able to validate reference tokens.
                options.EnableTokenEntryValidation = configuration.UseReferenceAccessTokens;

                // If an authority was explicitly set in the OpenID server options,
                // prefer it to the dynamic tenant comparison as it's more efficient.
                if (configuration.Authority != null)
                {
                    options.TokenValidationParameters.ValidIssuer = configuration.Authority.AbsoluteUri;
                }
                else
                {
                    options.TokenValidationParameters.IssuerValidator = (issuer, token, parameters) =>
                    {
                        if (!Uri.TryCreate(issuer, UriKind.Absolute, out Uri uri))
                        {
                            throw new SecurityTokenInvalidIssuerException("The token issuer is not valid.");
                        }

                        var tenant = _runningShellTable.Match(HostString.FromUriComponent(uri), uri.AbsolutePath);
                        if (tenant == null || !String.Equals(tenant.Name, settings.Tenant))
                        {
                            throw new SecurityTokenInvalidIssuerException("The token issuer is not valid.");
                        }

                        return issuer;
                    };
                }
            }).GetAwaiter().GetResult();
        }

        public void Configure(OpenIddictValidationDataProtectionOptions options)
        {
            var settings = GetValidationSettingsAsync().GetAwaiter().GetResult();
            if (settings == null)
            {
                return;
            }

            // If the tokens are issued by an authorization server located in a separate tenant,
            // resolve the isolated data protection provider associated with the specified tenant.
            if (!String.IsNullOrEmpty(settings.Tenant) &&
                !String.Equals(settings.Tenant, _shellSettings.Name))
            {
                CreateTenantScope(settings.Tenant).UsingAsync(async scope =>
                {
                    // If the other tenant is released, ensure the current tenant is also restarted as it
                    // relies on a data protection provider whose lifetime is managed by the other tenant.
                    // To make sure the other tenant is not disposed before all the pending requests are
                    // processed by the current tenant, a tenant dependency is manually added.
                    await scope.ShellContext.AddDependentShellAsync(await _shellHost.GetOrCreateShellContextAsync(_shellSettings));

                    // Note: the data protection provider is always registered as a singleton and thus will
                    // survive the current scope, which is mainly used to prevent the other tenant from being
                    // released before we have a chance to declare the current tenant as a dependent tenant.
                    options.DataProtectionProvider = scope.ServiceProvider.GetDataProtectionProvider();
                }).GetAwaiter().GetResult();
            }
        }

        public void Configure(string name, ApiAuthorizationOptions options)
        {
            // The default Orchard API authentication handler uses "Bearer" as the forwarded
            // authentication scheme, that corresponds to the default value used by the JWT
            // bearer handler from Microsoft. Yet, the OpenIddict validation handler uses
            // a different authentication scheme, so the API scheme must be manually replaced.
            options.ApiAuthenticationScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
        }

        public void Configure(ApiAuthorizationOptions options)
            => Debug.Fail("This infrastructure method shouldn't be called.");

        private ShellScope CreateTenantScope(string tenant)
        {
            // Optimization: if the specified name corresponds to the current tenant, use the current 'ShellScope'.
            if (String.IsNullOrEmpty(tenant) || String.Equals(tenant, _shellSettings.Name))
            {
                return ShellScope.Current;
            }

            return _shellHost.GetScopeAsync(tenant).GetAwaiter().GetResult();
        }

        private async Task<OpenIdServerSettings> GetServerSettingsAsync(IOpenIdServerService service)
        {
            var settings = await service.GetSettingsAsync();
            if ((await service.ValidateSettingsAsync(settings)).Any(result => result != ValidationResult.Success))
            {
                if (_shellSettings.IsRunning())
                {
                    _logger.LogWarning("The OpenID Connect module is not correctly configured.");
                }

                return null;
            }

            return settings;
        }

        private async Task<OpenIdValidationSettings> GetValidationSettingsAsync()
        {
            var settings = await _validationService.GetSettingsAsync();
            if ((await _validationService.ValidateSettingsAsync(settings)).Any(result => result != ValidationResult.Success))
            {
                if (_shellSettings.IsRunning())
                {
                    _logger.LogWarning("The OpenID Connect module is not correctly configured.");
                }

                return null;
            }

            return settings;
        }
    }
}
