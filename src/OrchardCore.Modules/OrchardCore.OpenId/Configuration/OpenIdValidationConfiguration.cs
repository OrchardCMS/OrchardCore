using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Validation;
using OpenIddict.Validation.Internal;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Modules;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;

namespace OrchardCore.OpenId.Configuration
{
    [Feature(OpenIdConstants.Features.Validation)]
    public class OpenIdValidationConfiguration : IConfigureOptions<AuthenticationOptions>,
        IConfigureNamedOptions<OpenIddictValidationOptions>,
        IConfigureNamedOptions<JwtBearerOptions>
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
                options.AddScheme<JwtBearerHandler>(JwtBearerDefaults.AuthenticationScheme, displayName: null);

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

                // Register the JWT or validation handler in the authentication handlers collection.
                if (configuration.AccessTokenFormat == OpenIdServerSettings.TokenFormat.Encrypted)
                {
                    options.AddScheme<OpenIddictValidationHandler>(OpenIddictValidationDefaults.AuthenticationScheme, displayName: null);
                }
                else if (configuration.AccessTokenFormat == OpenIdServerSettings.TokenFormat.JWT)
                {
                    options.AddScheme<JwtBearerHandler>(JwtBearerDefaults.AuthenticationScheme, displayName: null);
                }
                else
                {
                    throw new InvalidOperationException("The specified access token format is not valid.");
                }
            }).GetAwaiter().GetResult();
        }

        public void Configure(string name, JwtBearerOptions options)
        {
            // Ignore JWT handler instances that don't correspond to the instance managed by the OpenID module.
            if (!string.Equals(name, JwtBearerDefaults.AuthenticationScheme))
            {
                return;
            }

            var settings = GetValidationSettingsAsync().GetAwaiter().GetResult();
            if (settings == null)
            {
                return;
            }

            // If the tokens are issued by an authorization server located in an Orchard tenant, retrieve the
            // authority and the signing key and register them in the token validation parameters to prevent
            // the JWT handler from using an HTTP call to retrieve the discovery document from the other tenant.
            // Otherwise, set the authority to allow the JWT handler to retrieve the endpoint URLs/signing keys
            // from the remote server's metadata by sending an OpenID Connect/OAuth2 discovery request.

            if (settings.Authority != null)
            {
                options.RequireHttpsMetadata = string.Equals(settings.Authority.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);
                options.Audience = settings.Audience;
                options.Authority = settings.Authority.AbsoluteUri;

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

                // When the server is another tenant, don't allow the current tenant
                // to choose the valid audiences, as this would otherwise allow it
                // to validate/introspect tokens meant to be used with another tenant.
                options.TokenValidationParameters.ValidAudience = OpenIdConstants.Prefixes.Tenant + _shellSettings.Name;
                options.TokenValidationParameters.IssuerSigningKeys = await service.GetSigningKeysAsync();

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

                        var tenant = _runningShellTable.Match(new HostString(uri.Authority), uri.AbsolutePath);
                        if (tenant == null || !string.Equals(tenant.Name, settings.Tenant))
                        {
                            throw new SecurityTokenInvalidIssuerException("The token issuer is not valid.");
                        }

                        return issuer;
                    };
                }
            }).GetAwaiter().GetResult();
        }

        public void Configure(JwtBearerOptions options) => Debug.Fail("This infrastructure method shouldn't be called.");

        public void Configure(string name, OpenIddictValidationOptions options)
        {
            // Ignore validation handler instances that don't correspond to the instance managed by the OpenID module.
            if (!string.Equals(name, OpenIddictValidationDefaults.AuthenticationScheme))
            {
                return;
            }

            var settings = GetValidationSettingsAsync().GetAwaiter().GetResult();
            if (settings == null)
            {
                return;
            }

            // If the tokens are issued by an authorization server located in a separate tenant,
            // resolve the isolated data protection provider associated with the specified tenant.
            if (!string.IsNullOrEmpty(settings.Tenant) &&
                !string.Equals(settings.Tenant, _shellSettings.Name))
            {
                _shellHost.GetScopeAsync(settings.Tenant).GetAwaiter().GetResult().UsingAsync(async scope =>
                {
                    // If the other tenant is released, ensure the current tenant is also restarted as it
                    // relies on a data protection provider whose lifetime is managed by the other tenant.
                    // To make sure the other tenant is not disposed before all the pending requests are
                    // processed by the current tenant, a tenant dependency is manually added.
                    scope.ShellContext.AddDependentShell(await _shellHost.GetOrCreateShellContextAsync(_shellSettings));

                    // Note: the data protection provider is always registered as a singleton and thus will
                    // survive the current scope, which is mainly used to prevent the other tenant from being
                    // released before we have a chance to declare the current tenant as a dependent tenant.
                    options.DataProtectionProvider = scope.ServiceProvider.GetDataProtectionProvider();
                }).GetAwaiter().GetResult();
            }

            // Don't allow the current tenant to choose the valid audiences, as this would
            // otherwise allow it to introspect tokens meant to be used with another tenant.
            options.Audiences.Add(OpenIdConstants.Prefixes.Tenant + _shellSettings.Name);

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

                options.UseReferenceTokens = configuration.UseReferenceTokens;
            }).GetAwaiter().GetResult();
        }

        public void Configure(OpenIddictValidationOptions options) => Debug.Fail("This infrastructure method shouldn't be called.");

        private ShellScope CreateTenantScope(string tenant)
        {
            // Optimization: if the specified name corresponds to the current tenant, use the current 'ShellScope'.
            if (string.IsNullOrEmpty(tenant) || string.Equals(tenant, _shellSettings.Name))
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
                _logger.LogWarning("The OpenID Connect module is not correctly configured.");

                return null;
            }

            return settings;
        }

        private async Task<OpenIdValidationSettings> GetValidationSettingsAsync()
        {
            var settings = await _validationService.GetSettingsAsync();
            if ((await _validationService.ValidateSettingsAsync(settings)).Any(result => result != ValidationResult.Success))
            {
                _logger.LogWarning("The OpenID Connect module is not correctly configured.");

                return null;
            }

            return settings;
        }
    }
}
