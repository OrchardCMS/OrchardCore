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
    public class JwtBearerConfigureOptions : IConfigureOptions<JwtBearerOptions>
    {
        private readonly IOpenIdService _openIdService;

        public JwtBearerConfigureOptions(IOpenIdService openIdService)
        {
            _openIdService = openIdService;
        }

        public void Configure(JwtBearerOptions jwtBearerOptions)
        {   
            var openIdSettings = _openIdService.GetOpenIdSettingsAsync().Result;
            if (!_openIdService.IsValidOpenIdSettings(openIdSettings))
                return;
            
            if (openIdSettings.DefaultTokenFormat == OpenIdSettings.TokenFormat.JWT)
            {
                jwtBearerOptions.AutomaticAuthenticate = true;
                jwtBearerOptions.AutomaticChallenge = true;
                jwtBearerOptions.RequireHttpsMetadata = !openIdSettings.TestingModeEnabled;
                jwtBearerOptions.TokenValidationParameters.ValidAudiences = openIdSettings.Audiences;
                jwtBearerOptions.Authority = openIdSettings.Authority;
            }
        }
    }
}
