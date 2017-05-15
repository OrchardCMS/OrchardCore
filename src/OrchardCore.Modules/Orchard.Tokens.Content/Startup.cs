using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Tokens.Services;
using Orchard.Tokens.Content.Services;
using Orchard.Tokens.Content.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Orchard.Tokens.Content
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ISlugService, SlugService>();
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
            var tokenHelper = serviceProvider.GetRequiredService<TokensHelper>();
            tokenHelper.Handlebars.RegisterContentTokens(httpContextAccessor);
        }
    }
}
