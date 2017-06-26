using System.Linq;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Modules;
using Microsoft.Extensions.Options;

namespace Orchard.OpenId.Services
{
    [RequireFeatures("Orchard.Cors.Mvc")]
    public class OpenIdCorsConfiguration : IConfigureOptions<CorsOptions>
    {
        private readonly IOpenIdService _openIdService;

        public OpenIdCorsConfiguration(IOpenIdService openIdService)
        {
            _openIdService = openIdService;
        }

        public void Configure(CorsOptions options)
        {
            var openIdSettings = _openIdService.GetOpenIdSettingsAsync().GetAwaiter().GetResult();
            if (openIdSettings.Audiences != null && openIdSettings.Audiences.Any())
                options.AddPolicy(Constants.OpenIdConnectPolicy, builder => builder
                    .WithOrigins(openIdSettings.Audiences.ToArray())
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
                );
        }
    }
}