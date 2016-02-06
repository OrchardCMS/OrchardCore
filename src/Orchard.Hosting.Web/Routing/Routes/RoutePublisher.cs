using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Routing;
using Microsoft.AspNet.Routing.Template;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Environment.Shell;
using System;

namespace Orchard.Hosting.Web.Routing.Routes
{
    public class RoutePublisher : IRoutePublisher
    {
        private readonly IRouteBuilder _routeBuilder;
        private readonly ShellSettings _shellSettings;

        public RoutePublisher(
            IRouteBuilder routeBuilder,
            ShellSettings shellSettings)
        {
            _routeBuilder = routeBuilder;
            _shellSettings = shellSettings;
        }

        public void Publish(IEnumerable<RouteDescriptor> routes, RequestDelegate pipeline)
        {
            // Register one top level TenantRoute per tenant. Each instance contains all the routes
            // for this tenant.

            // In the case of several tenants, they will all be checked by ShellSettings. To optimize 
            // the TenantRoute resolution we can create a single Router type that would index the
            // TenantRoute object by their ShellSetting. This way there would just be one lookup.
            // And the ShellSettings test in TenantRoute would also be useless.

            var orderedRoutes = routes
                .OrderByDescending(r => r.Priority)
                .ToList();

            string routePrefix = "";
            if (!String.IsNullOrWhiteSpace(_shellSettings.RequestUrlPrefix))
            {
                routePrefix = _shellSettings.RequestUrlPrefix + "/";
            }

            // The default route is added to each tenant as a template route, with a prefix
            orderedRoutes.Add(new RouteDescriptor
            {
                Route = new Route("Default", "{area:exists}/{controller}/{action}/{id?}")
            });

            var inlineConstraint = _routeBuilder.ServiceProvider.GetService<IInlineConstraintResolver>();

            var templateRoutes = new List<IRouter>();

            foreach (var route in orderedRoutes)
            {
                IRouter router = new TemplateRoute(
                    _routeBuilder.DefaultHandler,
                    route.Route.RouteName,
                    routePrefix + route.Route.RouteTemplate,
                    route.Route.Defaults,
                    route.Route.Constraints,
                    route.Route.DataTokens,
                    inlineConstraint);

                templateRoutes.Add(router);
            }

            _routeBuilder.Routes.Add(new TenantRoute(_shellSettings, templateRoutes, pipeline));
        }
    }
}