using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.AspNetCore.Routing;
using Orchard.Environment.Shell;
using Orchard.Hosting.Web.Routing.Routes;
using Orchard.Routes;

namespace Orchard.Hosting
{
    public class OrchardShell : IOrchardShellEvents
    {
        private readonly ShellSettings _shellSettings;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEnumerable<IStartup> _startups;
        private readonly IRouteBuilder _routeBuilder;
        private readonly IInlineConstraintResolver _inlineConstraintResolver;

        public OrchardShell(
            IEnumerable<IStartup> startups,
            IRouteBuilder routeBuilder,
            IInlineConstraintResolver inlineConstraintResolver,
            ShellSettings shellSettings,
            IServiceProvider serviceProvider
            )
        {
            _inlineConstraintResolver = inlineConstraintResolver;
            _routeBuilder = routeBuilder;
            _startups = startups;
            _shellSettings = shellSettings;
            _serviceProvider = serviceProvider;
        }

        public Task ActivatingAsync()
        {
            // Build the middleware pipeline for the current tenant

            IApplicationBuilder appBuilder = new ApplicationBuilder(_serviceProvider);

            var prefixedRouteBuilder = new PrefixedRouteBuilder(_routeBuilder);

            // Register one top level TenantRoute per tenant. Each instance contains all the routes
            // for this tenant.

            // In the case of several tenants, they will all be checked by ShellSettings. To optimize
            // the TenantRoute resolution we can create a single Router type that would index the
            // TenantRoute object by their ShellSetting. This way there would just be one lookup.
            // And the ShellSettings test in TenantRoute would also be useless.

            foreach(var startup in _startups)
            {
                startup.Configure(appBuilder, prefixedRouteBuilder, _serviceProvider);
            }

            // Orchard is always the last middleware
            appBuilder.UseMiddleware<OrchardMiddleware>();

            var pipeline = appBuilder.Build();

            string routePrefix = "";
            if (!string.IsNullOrWhiteSpace(_shellSettings.RequestUrlPrefix))
            {
                routePrefix = _shellSettings.RequestUrlPrefix + "/";
            }

            // The default route is added to each tenant as a template route, with a prefix

            var templateRoutes = new List<Route>();

            templateRoutes.Add(new Route(
                _routeBuilder.DefaultHandler,
                "Default",
                routePrefix + "{area:exists}/{controller}/{action}/{id?}",
                null,
                null,
                null,
                _inlineConstraintResolver)
            );

            foreach (var route in prefixedRouteBuilder.Routes.OfType<Route>())
            {
                var constraints = new Dictionary<string, object>();

                foreach (var kv in route.Constraints)
                {
                    constraints.Add(kv.Key, kv.Value);
                }

                var prefixedRoute = new Route(
                    _routeBuilder.DefaultHandler,
                    route.Name,
                    routePrefix + route.RouteTemplate,
                    route.Defaults,
                    constraints,
                    route.DataTokens,
                    _inlineConstraintResolver);

                templateRoutes.Add(prefixedRoute);
            }

            _routeBuilder.Routes.Add(new TenantRoute(_shellSettings, templateRoutes, pipeline));

            return Task.CompletedTask;
        }

        public Task ActivatedAsync()
        {
            return Task.CompletedTask;
        }

        public Task TerminatingAsync()
        {
            return Task.CompletedTask;
        }

        public Task TerminatedAsync()
        {
            return Task.CompletedTask;
        }
    }
}