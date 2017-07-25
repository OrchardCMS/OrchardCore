using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Orchard.OpenId.Models;

namespace Orchard.OpenId.Services
{
    [RequireFeatures("Orchard.Cors.Mvc")]
    public class OpenIdCorsConfiguration : IConfigureOptions<CorsOptions>
    {
        private readonly OpenIdApplicationStore _openIdApplicationStore;

        public OpenIdCorsConfiguration(OpenIdApplicationStore openIdService)
        {
            _openIdApplicationStore = openIdService;
        }

        public void Configure(CorsOptions options)
        {
            var openIdApplications = _openIdApplicationStore.GetAllApps().GetAwaiter().GetResult().ToList();
            if (openIdApplications == null || openIdApplications.Count==0) return;
            var appOrigins = openIdApplications
                .Where(app => (app.AllowPasswordFlow || app.AllowClientCredentialsFlow || app.AllowRefreshTokenFlow) 
                              && app.AllowedOrigins != null && app.AllowedOrigins.Any())
                .SelectMany(app => app.AllowedOrigins)
                .ToArray();
            if (appOrigins.Length == 0)
            {
                return;
            }
            
            //Auth end-points policy
            options.AddPolicy(Constants.OpenIdConnectAuthPolicy, builder => builder
                .WithOrigins(appOrigins)
                .WithMethods("GET", "OPTIONS", "POST")
                .WithHeaders("accept", "content-type")
            );

            //UserInfo end-point policy
            options.AddPolicy(Constants.OpenIdConnectUserInfoPolicy, builder => builder
                .WithOrigins(appOrigins)
                .WithMethods("GET", "OPTIONS")
                .WithHeaders("accept", "content-type")
                .AllowCredentials()
            );
        }
    }
}