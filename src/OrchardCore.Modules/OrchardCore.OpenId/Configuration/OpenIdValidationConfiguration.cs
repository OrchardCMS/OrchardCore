using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Validation;
using OpenIddict.Validation.Internal;
using OrchardCore.Environment.Shell;
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
        private readonly ILogger<OpenIdValidationConfiguration> _logger;
        private readonly IOpenIdValidationService _validationService;
        private readonly IRunningShellTable _runningShellTable;
        private readonly IServiceProvider _serviceProvider;
        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;
        private readonly IShellSettingsManager _shellSettingsManager;

        public OpenIdValidationConfiguration(
            ILogger<OpenIdValidationConfiguration> logger,
            IOpenIdValidationService validationService,
            IRunningShellTable runningShellTable,
            IServiceProvider serviceProvider,
            IShellHost shellHost,
            ShellSettings shellSettings,
            IShellSettingsManager shellSettingsManager)
        {
            _logger = logger;
            _validationService = validationService;
            _runningShellTable = runningShellTable;
            _serviceProvider = serviceProvider;
            _shellHost = shellHost;
            _shellSettings = shellSettings;
            _shellSettingsManager = shellSettingsManager;
        }

        public void Configure(AuthenticationOptions options)
        {
            var settings = GetValidationSettingsAsync().GetAwaiter().GetResult();
            if (settings == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(settings.Authority))
            {
                options.AddScheme<JwtBearerHandler>(JwtBearerDefaults.AuthenticationScheme, displayName: null);

                return;
            }

            // Note: the shell host guarantees that the OpenID server service resolved inside
            // this using block won't be disposed until the service scope itself is released.
            using (var scope = CreateTenantScope(settings.Tenant))
            {
                var service = scope.ServiceProvider.GetService<IOpenIdServerService>();
                if (service == null)
                {
                    return;
                }

                var configuration = GetServerSettingsAsync(service).GetAwaiter().GetResult();
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
            }
        }

        public void Configure(string name, JwtBearerOptions options)
        {
            // Ignore JWT handler instances that don't correspond to the instance managed by the OpenID module.
            if (!string.Equals(name, JwtBearerDefaults.AuthenticationScheme, StringComparison.Ordinal))
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

            if (!string.IsNullOrEmpty(settings.Authority))
            {
                options.RequireHttpsMetadata = settings.Authority.StartsWith(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);
                options.Audience = settings.Audience;
                options.Authority = settings.Authority;

                return;
            }

            // Note: the shell host guarantees that the OpenID server service resolved inside
            // this using block won't be disposed until the service scope itself is released.
            using (var scope = CreateTenantScope(settings.Tenant))
            {
                var service = scope.ServiceProvider.GetService<IOpenIdServerService>();
                if (service == null)
                {
                    return;
                }

                var configuration = GetServerSettingsAsync(service).GetAwaiter().GetResult();
                if (configuration == null)
                {
                    return;
                }

                // When the server is another tenant, don't allow the current tenant
                // to choose the valid audiences, as this would otherwise allow it
                // to validate/introspect tokens meant to be used with another tenant.
                options.TokenValidationParameters.ValidAudience = OpenIdConstants.Prefixes.Tenant + _shellSettings.Name;
                options.TokenValidationParameters.IssuerSigningKeys = service.GetSigningKeysAsync().GetAwaiter().GetResult();

                // If an authority was explicitly set in the OpenID server options,
                // prefer it to the dynamic tenant comparison as it's more efficient.
                if (!string.IsNullOrEmpty(configuration.Authority))
                {
                    options.TokenValidationParameters.ValidIssuer = configuration.Authority;
                }
                else
                {
                    options.TokenValidationParameters.IssuerValidator = (issuer, token, parameters) =>
                    {
                        if (!Uri.TryCreate(issuer, UriKind.Absolute, out Uri uri))
                        {
                            throw new SecurityTokenInvalidIssuerException("The token issuer is not valid.");
                        }

                        var tenant = _runningShellTable.Match(uri.Authority, uri.AbsolutePath);
                        if (tenant == null || !string.Equals(tenant.Name, settings.Tenant, StringComparison.Ordinal))
                        {
                            throw new SecurityTokenInvalidIssuerException("The token issuer is not valid.");
                        }

                        return issuer;
                    };
                }
            }
        }

        public void Configure(JwtBearerOptions options) => Debug.Fail("This infrastructure method shouldn't be called.");

        public void Configure(string name, OpenIddictValidationOptions options)
        {
            // Ignore validation handler instances that don't correspond to the instance managed by the OpenID module.
            if (!string.Equals(name, OpenIddictValidationDefaults.AuthenticationScheme, StringComparison.Ordinal))
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
                !string.Equals(settings.Tenant, _shellSettings.Name, StringComparison.Ordinal))
            {
                var shellSettings = _shellSettingsManager.GetSettings(settings.Tenant);
                var (scope, shellContext) = _shellHost.GetScopeAndContextAsync(shellSettings).GetAwaiter().GetResult();
                using (scope)
                {
                    // If the other tenant is released, ensure the current tenant is also restarted as it
                    // relies on a data protection provider whose lifetime is managed by the other tenant.
                    // To make sure the other tenant is not disposed before all the pending requests are
                    // processed by the current tenant, a tenant dependency is manually added.
                    shellContext.AddDependentShell(_shellHost.GetOrCreateShellContextAsync(_shellSettings).GetAwaiter().GetResult());

                    // Note: the data protection provider is always registered as a singleton and thus will
                    // survive the current scope, which is mainly used to prevent the other tenant from being
                    // released before we have a chance to declare the current tenant as a dependent tenant.
                    options.DataProtectionProvider = scope.ServiceProvider.GetDataProtectionProvider();
                }
            }

            // Don't allow the current tenant to choose the valid audiences, as this would
            // otherwise allow it to introspect tokens meant to be used with another tenant.
            options.Audiences.Add(OpenIdConstants.Prefixes.Tenant + _shellSettings.Name);
        }

        public void Configure(OpenIddictValidationOptions options) => Debug.Fail("This infrastructure method shouldn't be called.");

        private IServiceScope CreateTenantScope(string tenant)
        {
            // Optimization: if the specified name corresponds to the current tenant, use the
            // service provider injected via the constructor instead of using the host APIs.
            if (string.IsNullOrEmpty(tenant) || string.Equals(tenant, _shellSettings.Name, StringComparison.Ordinal))
            {
                return _serviceProvider.CreateScope();
            }

            var settings = _shellSettingsManager.GetSettings(tenant);
            return _shellHost.GetScopeAsync(settings).GetAwaiter().GetResult();
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
