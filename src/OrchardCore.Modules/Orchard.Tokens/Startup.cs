using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Tokens.Services;

namespace Orchard.Tokens
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // Added as a singleton in order to reuse the cached templates
            services.AddSingleton<HandlebarsTokenizer>();
            services.AddScoped<ITokenizer, Tokenizer>();
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var handlebars = serviceProvider.GetRequiredService<HandlebarsTokenizer>().Handlebar;

            handlebars.RegisterStandardTokens();
        }
    }
}
