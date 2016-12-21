using AspNet.Security.OpenIdConnect.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenIddict;
using Orchard.OpenId.Services;
using Orchard.OpenId.Settings;
using Orchard.Settings;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.OpenId
{
    public class OpenIddictConfigureOptions : IConfigureOptions<OpenIddictOptions>
    {
        private readonly ILogger<OpenIddictConfigureOptions> _logger;
        private readonly IOpenIdService _openIdService;

        public OpenIddictConfigureOptions(ILogger<OpenIddictConfigureOptions> logger, ISiteService siteService, IOpenIdService openIdService)
        {
            _logger = logger;
            _openIdService = openIdService;
        }

        public void Configure(OpenIddictOptions openIddictOptions)
        {   
            var openIdSettings = _openIdService.GetOpenIdSettingsAsync().Result;
            if (!_openIdService.IsValidOpenIdSettings(openIdSettings))
                return;
            
            if (openIdSettings.DefaultTokenFormat == OpenIdSettings.TokenFormat.JWT)
                openIddictOptions.AccessTokenHandler = new JwtSecurityTokenHandler();

            if (openIdSettings.TestingModeEnabled)
            {
                openIddictOptions.SigningCredentials.AddEphemeralKey();
                openIddictOptions.AllowInsecureHttp = true;
            }
            else if(openIdSettings.CertificateStoreLocation.HasValue && openIdSettings.CertificateStoreName.HasValue && !string.IsNullOrEmpty(openIdSettings.CertificateThumbPrint))
            {
                try
                {
                    openIddictOptions.AllowInsecureHttp = false;
                    openIddictOptions.SigningCredentials.Clear();
                    openIddictOptions.SigningCredentials.AddCertificate(openIdSettings.CertificateThumbPrint, openIdSettings.CertificateStoreName.Value, openIdSettings.CertificateStoreLocation.Value);
                }
                catch (Exception e)
                {
                    _logger.LogError("A valid signing certificate is required for Orchard.OpenId module.", e);
                    throw;
                }
            }

            if (openIdSettings.EnableAuthorizationEndpoint)
            {
                openIddictOptions.AuthorizationEndpointPath = "/Orchard.OpenId/Access/Authorize";
            }
            if (openIdSettings.EnableTokenEndpoint)
            {
                openIddictOptions.TokenEndpointPath = "/Orchard.OpenId/Access/Token";
            }
            if (openIdSettings.EnableLogoutEndpoint)
            {
                openIddictOptions.LogoutEndpointPath = "/Orchard.OpenId/Access/Logout";
            }
            if (openIdSettings.EnableUserInfoEndpoint)
            {
                openIddictOptions.UserinfoEndpointPath = "/Orchard.OpenId/Access/Userinfo";
            }
            if (openIdSettings.AllowPasswordFlow)
            {
                openIddictOptions.GrantTypes.Add(OpenIdConnectConstants.GrantTypes.Password);
            }
            if (openIdSettings.AllowClientCredentialsFlow)
            {
                openIddictOptions.GrantTypes.Add(OpenIdConnectConstants.GrantTypes.ClientCredentials);
            }
            if (openIdSettings.AllowAuthorizationCodeFlow || openIdSettings.AllowHybridFlow)
            {
                openIddictOptions.GrantTypes.Add(OpenIdConnectConstants.GrantTypes.AuthorizationCode);
            }
            if (openIdSettings.AllowRefreshTokenFlow)
            {
                openIddictOptions.GrantTypes.Add(OpenIdConnectConstants.GrantTypes.RefreshToken);
            }
            if (openIdSettings.AllowImplicitFlow || openIdSettings.AllowHybridFlow)
            {
                openIddictOptions.GrantTypes.Add(OpenIdConnectConstants.GrantTypes.Implicit);
            }
        }
    }
}
