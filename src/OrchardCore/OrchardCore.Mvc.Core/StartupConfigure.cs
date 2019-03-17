using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;

namespace OrchardCore.Mvc
{
    public class StartupConfigure : StartupBase
    {
        public override int Order => 200;

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var inlineConstraintResolver = routes.ServiceProvider.GetService<IInlineConstraintResolver>();

            // The default route is added to each tenant as a template route, with a prefix
            routes.Routes.Add(new Route(
                routes.DefaultHandler,
                "Default",
                "{area:exists}/{controller}/{action}/{id?}",
                null,
                null,
                null,
                inlineConstraintResolver)
            );

            routes.Routes.Insert(0, AttributeRouting.CreateAttributeMegaRoute(routes.ServiceProvider));
        }
    }
}
