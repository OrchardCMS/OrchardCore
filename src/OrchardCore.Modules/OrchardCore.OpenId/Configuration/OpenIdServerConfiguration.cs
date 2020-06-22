using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Mvc;
using OpenIddict.Server;
using OpenIddict.Server.Internal;
using OpenIddict.Validation;
using OpenIddict.Validation.Internal;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;

namespace OrchardCore.OpenId.Configuration
{
    [Feature(OpenIdConstants.Features.Server)]
    public class OpenIdServerConfiguration : IConfigureOptions<AuthenticationOptions>,
        IConfigureOptions<OpenIddictMvcOptions>,
        IConfigureNamedOptions<OpenIddictServerOptions>,
        IConfigureNamedOptions<OpenIddictValidationOptions>,
        IConfigureNamedOptions<JwtBearerOptions>
    {
        private readonly ILogger _logger;
        private readonly IRunningShellTable _runningShellTable;
        private readonly ShellSettings _shellSettings;
        private readonly IOpenIdServerService _serverService;

        public OpenIdServerConfiguration(
            ILogger<OpenIdServerConfiguration> logger,
            IRunningShellTable runningShellTable,
            ShellSettings shellSettings,
            IOpenIdServerService serverService)
        {
            _logger = logger;
            _runningShellTable = runningShellTable;
            _shellSettings = shellSettings;
            _serverService = serverService;
        }

        public void Configure(AuthenticationOptions options)
        {
            var settings = GetServerSettingsAsync().GetAwaiter().GetResult();
            if (settings == null)
            {
                return;
            }

            // Register the OpenIddict handler in the authentication handlers collection.
            options.AddScheme<OpenIddictServerHandler>(OpenIddictServerDefaults.AuthenticationScheme, displayName: null);

            // If the userinfo endpoint was enabled, register a private JWT or validation handler instance.
            // Unlike the instance registered by the validation feature, this one is only used for the
            // OpenID Connect userinfo endpoint and thus only supports local opaque/JWT token validation.
            if (settings.UserinfoEndpointPath.HasValue)
            {
                if (settings.AccessTokenFormat == OpenIdServerSettings.TokenFormat.Encrypted)
                {
                    options.AddScheme<OpenIddictValidationHandler>(OpenIdConstants.Schemes.Userinfo, displayName: null);
                }
                else if (settings.AccessTokenFormat == OpenIdServerSettings.TokenFormat.JWT)
                {
                    options.AddScheme<JwtBearerHandler>(OpenIdConstants.Schemes.Userinfo, displayName: null);
                }
                else
                {
                    throw new InvalidOperationException("The specified access token format is not valid.");
                }
            }
        }

        // Note: to ensure no exception is thrown while binding OpenID Connect primitives
        // when the OpenID server settings are invalid, the binding exceptions that are
        // thrown by OpenIddict to indicate the request cannot be extracted are turned off.
        public void Configure(OpenIddictMvcOptions options) => options.DisableBindingExceptions = true;

        public void Configure(string name, OpenIddictServerOptions options)
        {
            // Ignore OpenIddict handler instances that don't correspond to the instance managed by the OpenID module.
            if (!string.Equals(name, OpenIddictServerDefaults.AuthenticationScheme))
            {
                return;
            }

            var settings = GetServerSettingsAsync().GetAwaiter().GetResult();
            if (settings == null)
            {
                return;
            }

            // Note: in Orchard, transport security is usually configured via the dedicated HTTPS module.
            // To make configuration easier and avoid having to configure it in two different features,
            // the transport security requirement enforced by OpenIddict by default is always turned off.
            options.AllowInsecureHttp = true;

            options.ApplicationCanDisplayErrors = true;
            options.EnableRequestCaching = true;
            options.IgnoreScopePermissions = true;
            options.Issuer = settings.Authority;
            options.UseRollingTokens = settings.UseRollingTokens;
            options.UseReferenceTokens = settings.UseReferenceTokens;

            foreach (var key in _serverService.GetSigningKeysAsync().GetAwaiter().GetResult())
            {
                options.SigningCredentials.AddKey(key);
            }

            if (settings.AccessTokenFormat == OpenIdServerSettings.TokenFormat.JWT)
            {
                options.AccessTokenHandler = new JwtSecurityTokenHandler();
            }

            options.AuthorizationEndpointPath = settings.AuthorizationEndpointPath;
            options.LogoutEndpointPath = settings.LogoutEndpointPath;
            options.TokenEndpointPath = settings.TokenEndpointPath;
            options.UserinfoEndpointPath = settings.UserinfoEndpointPath;

            options.GrantTypes.Clear();
            options.GrantTypes.UnionWith(settings.GrantTypes);

            options.Scopes.Add(OpenIddictConstants.Scopes.Email);
            options.Scopes.Add(OpenIddictConstants.Scopes.Phone);
            options.Scopes.Add(OpenIddictConstants.Scopes.Profile);
            options.Scopes.Add(OpenIddictConstants.Claims.Roles);
        }

        public void Configure(OpenIddictServerOptions options) => Debug.Fail("This infrastructure method shouldn't be called.");

        public void Configure(string name, JwtBearerOptions options)
        {
            // Ignore JWT handler instances that don't correspond to the private instance managed by the OpenID module.
            if (!string.Equals(name, OpenIdConstants.Schemes.Userinfo))
            {
                return;
            }

            var settings = GetServerSettingsAsync().GetAwaiter().GetResult();
            if (settings == null)
            {
                return;
            }

            options.TokenValidationParameters.ValidAudience = OpenIdConstants.Prefixes.Tenant + _shellSettings.Name;
            options.TokenValidationParameters.IssuerSigningKeys = _serverService.GetSigningKeysAsync().GetAwaiter().GetResult();

            // If an authority was explicitly set in the OpenID server options,
            // prefer it to the dynamic tenant comparison as it's more efficient.
            if (settings.Authority != null)
            {
                options.TokenValidationParameters.ValidIssuer = settings.Authority.AbsoluteUri;
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
                    if (tenant == null || !string.Equals(tenant.Name, _shellSettings.Name))
                    {
                        throw new SecurityTokenInvalidIssuerException("The token issuer is not valid.");
                    }

                    return issuer;
                };
            }
        }

        public void Configure(JwtBearerOptions options) => Debug.Fail("This infrastructure method shouldn't be called.");

        public void Configure(string name, OpenIddictValidationOptions options)
        {
            // Ignore validation handler instances that don't correspond to the private instance managed by the OpenID module.
            if (!string.Equals(name, OpenIdConstants.Schemes.Userinfo))
            {
                return;
            }

            options.Audiences.Add(OpenIdConstants.Prefixes.Tenant + _shellSettings.Name);

            var serverSettings = GetServerSettingsAsync().GetAwaiter().GetResult();
            if (serverSettings == null)
            {
                return;
            }
            options.UseReferenceTokens = serverSettings.UseReferenceTokens;
        }

        public void Configure(OpenIddictValidationOptions options) => Debug.Fail("This infrastructure method shouldn't be called.");

        private async Task<OpenIdServerSettings> GetServerSettingsAsync()
        {
            var settings = await _serverService.GetSettingsAsync();
            if ((await _serverService.ValidateSettingsAsync(settings)).Any(result => result != ValidationResult.Success))
            {
                _logger.LogWarning("The OpenID Connect module is not correctly configured.");

                return null;
            }

            return settings;
        }
    }
}
