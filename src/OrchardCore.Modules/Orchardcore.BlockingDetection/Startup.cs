using System;
using Ben.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Modules;

namespace Orchardcore.BlockingDetection
{
    public class Startup : StartupBase
    {
        public override int Order => int.MinValue + 100;
        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            app.UseBlockingDetection();
        }        
    }
}
