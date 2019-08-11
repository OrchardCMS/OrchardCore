using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Modules;

namespace OrchardCore.Mvc
{
    public class ModularTenantRouteBuilder : IModularTenantRouteBuilder
    {
        // Register one top level TenantRoute per tenant. Each instance contains all the routes
        // for this tenant.
        public ModularTenantRouteBuilder()
        {
        }

        public IRouteBuilder Build(IApplicationBuilder appBuilder)
        {
            var routeBuilder = new RouteBuilder(appBuilder)
            {
                DefaultHandler = appBuilder.ApplicationServices.GetRequiredService<MvcRouteHandler>()
            };

            return routeBuilder;
        }

        public void Configure(IRouteBuilder builder)
        {
            var inlineConstraintResolver = builder.ServiceProvider.GetService<IInlineConstraintResolver>();

            // The default route is added to each tenant as a template route, with a prefix
            builder.Routes.Add(new Route(
                builder.DefaultHandler,
                "Default",
                "{area:exists}/{controller}/{action}/{id?}",
                null,
                null,
                null,
                inlineConstraintResolver)
            );

            builder.Routes.Insert(0, AttributeRouting.CreateAttributeMegaRoute(builder.ServiceProvider));
        }
    }
}