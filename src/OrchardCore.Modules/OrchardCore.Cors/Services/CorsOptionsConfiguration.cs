using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Options;
using System.Linq;

namespace OrchardCore.Cors.Services
{
    public class CorsOptionsConfiguration : IConfigureOptions<CorsOptions>
    {
        private readonly CorsService _corsService;

        public CorsOptionsConfiguration(CorsService corsService)
        {
            _corsService = corsService;
        }

        public void Configure(CorsOptions options)
        {
            var corsSettings = _corsService.GetSettingsAsync().GetAwaiter().GetResult();
            if(corsSettings?.Polices == null)
                return;

            foreach (var corsPolicy in corsSettings.Polices)
            {
                options.AddPolicy(corsPolicy.Name, configurePolicy =>
                {
                    if (corsPolicy.AllowAnyHeader)
                    {
                        configurePolicy.AllowAnyHeader();
                    }
                    else
                    {
                        configurePolicy.WithHeaders(corsPolicy.AllowedOrigins);
                    }

                    if (corsPolicy.AllowAnyMethod)
                    {
                        configurePolicy.AllowAnyMethod();
                    }
                    else
                    {
                        configurePolicy.WithMethods(corsPolicy.AllowedMethods);
                    }

                    if (corsPolicy.AllowAnyOrigin)
                    {
                        configurePolicy.AllowAnyOrigin();
                    }
                    else
                    {
                        configurePolicy.WithOrigins(corsPolicy.AllowedOrigins);
                    }

                    if (corsPolicy.AllowCredentials)
                    {
                        configurePolicy.AllowCredentials();
                    }
                    else
                    {
                        configurePolicy.DisallowCredentials();
                    }
                });
            }

            options.DefaultPolicyName = corsSettings.Polices.First().Name;
        }
    }
}
