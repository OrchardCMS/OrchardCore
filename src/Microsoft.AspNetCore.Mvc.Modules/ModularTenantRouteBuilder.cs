using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.AspNetCore.Modules;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Mvc.Modules
{
    public class ModularTenantRouteBuilder : IModularTenantRouteBuilder
    {
        private readonly IServiceProvider _serviceProvider;

        // Register one top level TenantRoute per tenant. Each instance contains all the routes
        // for this tenant.
        public ModularTenantRouteBuilder(IServiceProvider serviceProvider)
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
        }
    }
}
