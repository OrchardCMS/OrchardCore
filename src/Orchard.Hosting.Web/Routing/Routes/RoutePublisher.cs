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
            var orderedRoutes = routes
                .OrderByDescending(r => r.Priority)
                .ToList();

            string routePrefix = "";
            if (!String.IsNullOrWhiteSpace(_shellSettings.RequestUrlPrefix))
            {
                routePrefix = _shellSettings.RequestUrlPrefix + "/";
            }

            orderedRoutes.Insert(0, new RouteDescriptor
            {
                Route = new Route("Default", "{area}/{controller}/{action}/{id?}")
            });


            var inlineConstraint = _routeBuilder.ServiceProvider.GetService<IInlineConstraintResolver>();

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

                _routeBuilder.Routes.Add(new TenantRoute(_shellSettings, router, pipeline));
            }
        }
    }
}