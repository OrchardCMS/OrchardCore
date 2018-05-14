using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;

namespace OrchardCore.Authentication
{
    public class Startup : StartupBase
    {
        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            //app.UseAuthentication();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            //services.AddAuthentication();

            // Note: IAuthenticationSchemeProvider is already registered at the host level.
            // We need to register it again so it is taken into account at the tenant level.
            //services.AddSingleton<IAuthenticationSchemeProvider, AuthenticationSchemeProvider>();
        }
    }
}