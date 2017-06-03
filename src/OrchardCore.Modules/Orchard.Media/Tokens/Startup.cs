using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Modules;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Tokens;

namespace Orchard.Media.Tokens
{
    [RequireFeatures("Orchard.Media")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
            var tokenHelper = serviceProvider.GetRequiredService<TokensHelper>();
            tokenHelper.Handlebars.RegisterMediaTokens(httpContextAccessor);
        }
    }
}
