using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.AspNetCore.Modules.Routing;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Settings;

namespace Microsoft.AspNetCore.Mvc.Modules.Routing
{
    public class MvcTenantRouteBuilder : ITenantRouteBuilder
    {
        private readonly IServiceProvider _serviceProvider;

        public MvcTenantRouteBuilder(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IRouteBuilder Build()
        {
            IApplicationBuilder appBuilder = new ApplicationBuilder(_serviceProvider);

            var routeBuilder = new RouteBuilder(appBuilder)
            {
                DefaultHandler = _serviceProvider.GetRequiredService<MvcRouteHandler>()
            };

            return routeBuilder;
        }

        public void Configure(IRouteBuilder builder)
        {
            var inlineConstraintResolver = _serviceProvider.GetService<IInlineConstraintResolver>();

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

            var siteService = builder.ServiceProvider.GetService<ISiteService>();

            // ISiteService might not be registered during Setup
            if (siteService != null)
            {
                // Add home page route
                builder.Routes.Add(new HomePageRoute(siteService, builder, inlineConstraintResolver));
            }
        }
    }
}
