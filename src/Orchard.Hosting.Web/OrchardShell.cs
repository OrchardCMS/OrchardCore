using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Builder.Internal;
using Orchard.Hosting.Middleware;
using Orchard.Hosting.Web.Routing.Routes;
using Orchard.Events;
using Orchard.Environment.Shell;

namespace Orchard.Hosting {
    public class OrchardShell : IOrchardShell {
        private readonly IEnumerable<IRouteProvider> _routeProviders;
        private readonly IRoutePublisher _routePublisher;
        private readonly IEnumerable<IMiddlewareProvider> _middlewareProviders;
        private readonly ShellSettings _shellSettings;
        private readonly IServiceProvider _serviceProvider;
        private readonly IEventNotifier _eventNotifier;

        public OrchardShell (
            IEnumerable<IRouteProvider> routeProviders,
            IRoutePublisher routePublisher,
            IEnumerable<IMiddlewareProvider> middlewareProviders,
            ShellSettings shellSettings,
            IServiceProvider serviceProvider,
            IEventNotifier eventNotifier) {
            _routeProviders = routeProviders;
            _routePublisher = routePublisher;
            _middlewareProviders = middlewareProviders;
            _shellSettings = shellSettings;
            _serviceProvider = serviceProvider;
            _eventNotifier = eventNotifier;
        }

        public void Activate() {
            IApplicationBuilder appBuilder = new ApplicationBuilder(_serviceProvider);
            
            appBuilder.Properties["host.AppName"] = _shellSettings.Name;

            var orderedMiddlewares = _middlewareProviders
                .SelectMany(p => p.GetMiddlewares())
                .OrderBy(obj => obj.Priority);

            foreach (var middleware in orderedMiddlewares) {
                middleware.Configure(appBuilder);
            }

            appBuilder.UseOrchard();

            var pipeline = appBuilder.Build();

            var allRoutes = _routeProviders.SelectMany(provider => provider.GetRoutes()).ToArray();

            _routePublisher.Publish(allRoutes, pipeline);

            _eventNotifier.Notify<IOrchardShellEvents>(x => x.Activated());
        }

        public void Terminate() {
            _eventNotifier.Notify<IOrchardShellEvents>(x => x.Terminating());
        }
    }
}