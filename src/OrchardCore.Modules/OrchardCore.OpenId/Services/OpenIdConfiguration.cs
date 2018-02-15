using System;
using System.IdentityModel.Tokens.Jwt;
using AspNet.Security.OAuth.Validation;
using AspNet.Security.OpenIdConnect.Primitives;
using AspNet.Security.OpenIdConnect.Server;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenIddict;
using OrchardCore.Environment.Shell;
using OrchardCore.OpenId.Abstractions.Models;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;

namespace OrchardCore.OpenId
{
    public class OpenIdConfiguration : IConfigureOptions<AuthenticationOptions>,
        IConfigureNamedOptions<OpenIddictOptions>,
        IConfigureNamedOptions<JwtBearerOptions>,
        IConfigureNamedOptions<OAuthValidationOptions>
    {
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly ILogger<OpenIdConfiguration> _logger;
        private readonly IOpenIdService _openIdService;

        public OpenIdConfiguration(
            IDataProtectionProvider dataProtectionProvider,
            ILogger<OpenIdConfiguration> logger,
            IOpenIdService openIdService,
            ShellSettings shellSettings)
        {
            _dataProtectionProvider = dataProtectionProvider.CreateProtector(shellSettings.Name);
            _logger = logger;
            _openIdService = openIdService;
        }

        public void Configure(AuthenticationOptions options)
        {
            var settings = _openIdService.GetOpenIdSettingsAsync().GetAwaiter().GetResult();
            if (!_openIdService.IsValidOpenIdSettings(settings))
            {
                _logger.LogWarning("The OpenID Connect module is not correctly configured.");
                return;
            }

            // Register the OpenIddict handler in the authentication handlers collection.
            options.AddScheme(OpenIdConnectServerDefaults.AuthenticationScheme, builder =>
            {
                builder.HandlerType = typeof(OpenIddictHandler);
            });

            // Register the JWT or validation handler in the authentication handlers collection.
            if (settings.AccessTokenFormat == OpenIdSettings.TokenFormat.Encrypted)
            {
                options.AddScheme(OAuthValidationDefaults.AuthenticationScheme, builder =>
                {
                    builder.HandlerType = typeof(OAuthValidationHandler);
                });
            }
            else if (settings.AccessTokenFormat == OpenIdSettings.TokenFormat.JWT)
            {
                // Note: unlike most authentication handlers in ASP.NET Core 2.0,
                // the JWT bearer handler is not public (which is likely an oversight).
                // To work around this issue, the handler type is resolved using reflection.
                options.AddScheme(JwtBearerDefaults.AuthenticationScheme, builder =>
                {
                    builder.HandlerType = typeof(JwtBearerOptions).Assembly
                        .GetType("Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerHandler");
                });
            }
            else
            {
                throw new InvalidOperationException("The specified access token format is not valid.");
            }
        }

        public void Configure(string name, OpenIddictOptions options)
        {
            // Ignore OpenIddict handler instances that don't correspond to the instance managed by the OpenID module.
            if (!string.Equals(name, OpenIdConnectServerDefaults.AuthenticationScheme, StringComparison.Ordinal))
            {
                return;
            }

            var settings = _openIdService.GetOpenIdSettingsAsync().GetAwaiter().GetResult();
            if (!_openIdService.IsValidOpenIdSettings(settings))
            {
                return;
            }

            options.ProviderType = typeof(OpenIddictProvider<IOpenIdApplication, IOpenIdAuthorization, IOpenIdScope, IOpenIdToken>);
            options.DataProtectionProvider = _dataProtectionProvider;
            options.ApplicationCanDisplayErrors = true;
            options.EnableRequestCaching = true;
            options.RequireClientIdentification = true;

            if (settings.AccessTokenFormat == OpenIdSettings.TokenFormat.JWT)
            {
                options.AccessTokenHandler = new JwtSecurityTokenHandler();
            }

            options.UseRollingTokens = settings.UseRollingTokens;

            if (settings.TestingModeEnabled)
            {
                options.SigningCredentials.AddEphemeralKey();
                options.AllowInsecureHttp = true;
            }
            else if (settings.CertificateStoreLocation.HasValue && settings.CertificateStoreName.HasValue && !string.IsNullOrEmpty(settings.CertificateThumbPrint))
            {
                try
                {
                    options.AllowInsecureHttp = false;
                    options.SigningCredentials.Clear();
                    options.SigningCredentials.AddCertificate(settings.CertificateThumbPrint, settings.CertificateStoreName.Value, settings.CertificateStoreLocation.Value);
                }
                catch (Exception exception)
                {
                    _logger.LogError("An error occurred while trying to register a X.509 certificate.", exception);
                    throw;
                }
            }

            if (settings.EnableAuthorizationEndpoint)
            {
                options.AuthorizationEndpointPath = "/OrchardCore.OpenId/Access/Authorize";
            }
            if (settings.EnableTokenEndpoint)
            {
                options.TokenEndpointPath = "/OrchardCore.OpenId/Access/Token";
            }
            if (settings.EnableLogoutEndpoint)
            {
                options.LogoutEndpointPath = "/OrchardCore.OpenId/Access/Logout";
            }
            if (settings.EnableUserInfoEndpoint)
            {
                options.UserinfoEndpointPath = "/OrchardCore.OpenId/UserInfo/Me";
            }
            if (settings.AllowAuthorizationCodeFlow)
            {
                options.GrantTypes.Add(OpenIdConnectConstants.GrantTypes.AuthorizationCode);
            }
            if (settings.AllowClientCredentialsFlow)
            {
                options.GrantTypes.Add(OpenIdConnectConstants.GrantTypes.ClientCredentials);
            }
            if (settings.AllowImplicitFlow)
            {
                options.GrantTypes.Add(OpenIdConnectConstants.GrantTypes.Implicit);
            }
            if (settings.AllowPasswordFlow)
            {
                options.GrantTypes.Add(OpenIdConnectConstants.GrantTypes.Password);
            }
            if (settings.AllowRefreshTokenFlow)
            {
                options.GrantTypes.Add(OpenIdConnectConstants.GrantTypes.RefreshToken);
            }
        }

        public void Configure(OpenIddictOptions options) { }

        public void Configure(string name, JwtBearerOptions options)
        {
            // Ignore JWT handler instances that don't correspond to the instance managed by the OpenID module.
            if (!string.Equals(name, JwtBearerDefaults.AuthenticationScheme, StringComparison.Ordinal))
            {
                return;
            }

            var settings = _openIdService.GetOpenIdSettingsAsync().GetAwaiter().GetResult();
            if (!_openIdService.IsValidOpenIdSettings(settings))
            {
                return;
            }

            options.RequireHttpsMetadata = !settings.TestingModeEnabled;
            options.Authority = settings.Authority;
            options.TokenValidationParameters.ValidAudiences = settings.Audiences;
        }

        public void Configure(JwtBearerOptions options) { }

        public void Configure(string name, OAuthValidationOptions options)
        {
            // Ignore validation handler instances that don't correspond to the instance managed by the OpenID module.
            if (!string.Equals(name, OAuthValidationDefaults.AuthenticationScheme, StringComparison.Ordinal))
            {
                return;
            }

            var settings = _openIdService.GetOpenIdSettingsAsync().GetAwaiter().GetResult();
            if (!_openIdService.IsValidOpenIdSettings(settings))
            {
                return;
            }

            options.Audiences.UnionWith(settings.Audiences);
            options.DataProtectionProvider = _dataProtectionProvider;
        }

        public void Configure(OAuthValidationOptions options) { }
    }
}
