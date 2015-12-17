using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Builder.Internal;
using Orchard.Hosting.Middleware;
using Orchard.Hosting.Web.Routing.Routes;
using Orchard.Environment.Shell;
using System.Threading.Tasks;

namespace Orchard.Hosting
{
    public class OrchardShell : IOrchardShellEvents
    {
        private readonly IEnumerable<IRouteProvider> _routeProviders;
        private readonly IRoutePublisher _routePublisher;
        private readonly IEnumerable<IMiddlewareProvider> _middlewareProviders;
        private readonly ShellSettings _shellSettings;
        private readonly IServiceProvider _serviceProvider;

        public OrchardShell(
            IEnumerable<IRouteProvider> routeProviders,
            IRoutePublisher routePublisher,
            IEnumerable<IMiddlewareProvider> middlewareProviders,
            ShellSettings shellSettings,
            IServiceProvider serviceProvider
            )
        {
            _routeProviders = routeProviders;
            _routePublisher = routePublisher;
            _middlewareProviders = middlewareProviders;
            _shellSettings = shellSettings;
            _serviceProvider = serviceProvider;
        }

        public Task ActivatingAsync()
        {
            IApplicationBuilder appBuilder = new ApplicationBuilder(_serviceProvider);

            var orderedMiddlewares = _middlewareProviders
                .SelectMany(p => p.GetMiddlewares())
                .OrderBy(obj => obj.Priority)
                .ToArray();

            RequestDelegate pipeline = null;

            // If there are custom middleware for this tenant,
            // build a custom pipeline for its routes
            if (orderedMiddlewares.Length > 0)
            {
                foreach (var middleware in orderedMiddlewares)
                {
                    middleware.Configure(appBuilder);
                }

                pipeline = appBuilder.Build();
            }

            _routePublisher.Publish(
                _routeProviders.SelectMany(provider => provider.GetRoutes()),
                pipeline
            );

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