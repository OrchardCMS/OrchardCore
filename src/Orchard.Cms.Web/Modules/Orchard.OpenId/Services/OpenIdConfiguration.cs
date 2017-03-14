using System;
using System.IdentityModel.Tokens.Jwt;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenIddict;
using Orchard.OpenId.Services;
using Orchard.OpenId.Settings;

namespace Orchard.OpenId
{
    public class OpenIdConfiguration : IConfigureOptions<OpenIddictOptions>
    {
        private readonly ILogger<OpenIdConfiguration> _logger;
        private readonly IOpenIdService _openIdService;

        public OpenIdConfiguration(ILogger<OpenIdConfiguration> logger, IOpenIdService openIdService)
        {
            _logger = logger;
            _openIdService = openIdService;
        }

        public void Configure(OpenIddictOptions options)
        {   
            var settings = _openIdService.GetOpenIdSettingsAsync().GetAwaiter().GetResult();
            if (!_openIdService.IsValidOpenIdSettings(settings))
            {
                _logger.LogWarning("The OpenID Connect module is not correctly configured.");
                return;
            }

            if (settings.AccessTokenFormat == OpenIdSettings.TokenFormat.JWT)
            {
                options.AccessTokenHandler = new JwtSecurityTokenHandler();
            }

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
                options.AuthorizationEndpointPath = "/Orchard.OpenId/Access/Authorize";
            }
            if (settings.EnableTokenEndpoint)
            {
                options.TokenEndpointPath = "/Orchard.OpenId/Access/Token";
            }
            if (settings.EnableLogoutEndpoint)
            {
                options.LogoutEndpointPath = "/Orchard.OpenId/Access/Logout";
            }
            if (settings.EnableUserInfoEndpoint)
            {
                options.UserinfoEndpointPath = "/Orchard.OpenId/UserInfo/Me";
            }
            if (settings.AllowPasswordFlow)
            {
                options.GrantTypes.Add(OpenIdConnectConstants.GrantTypes.Password);
            }
            if (settings.AllowClientCredentialsFlow)
            {
                options.GrantTypes.Add(OpenIdConnectConstants.GrantTypes.ClientCredentials);
            }
            if (settings.AllowAuthorizationCodeFlow || settings.AllowHybridFlow)
            {
                options.GrantTypes.Add(OpenIdConnectConstants.GrantTypes.AuthorizationCode);
            }
            if (settings.AllowRefreshTokenFlow)
            {
                options.GrantTypes.Add(OpenIdConnectConstants.GrantTypes.RefreshToken);
            }
            if (settings.AllowImplicitFlow || settings.AllowHybridFlow)
            {
                options.GrantTypes.Add(OpenIdConnectConstants.GrantTypes.Implicit);
            } 
        }
    }
}
