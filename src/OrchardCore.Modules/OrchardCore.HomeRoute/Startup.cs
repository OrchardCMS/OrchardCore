using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;

namespace OrchardCore.HomeRoute
{
    public class Startup : StartupBase
    {
        public override int Order => -100;

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var inlineConstraintResolver = serviceProvider.GetService<IInlineConstraintResolver>();
            routes.Routes.Add(new HomePageRoute(routes, inlineConstraintResolver));
            app.UseMiddleware<HomeRouteMiddleware>();
        }
    }
}
