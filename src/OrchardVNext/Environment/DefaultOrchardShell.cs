using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Builder;
using OrchardVNext.Environment.Configuration;
using OrchardVNext.Middleware;
using OrchardVNext.Mvc.Routes;

namespace OrchardVNext.Environment {
    public interface IOrchardShell {
        void Activate();
        void Terminate();
    }

    public class DefaultOrchardShell : IOrchardShell {
        private readonly IEnumerable<IRouteProvider> _routeProviders;
        private readonly IRoutePublisher _routePublisher;
        private readonly IEnumerable<IMiddlewareProvider> _middlewareProviders;
        private readonly ShellSettings _shellSettings;
        private readonly IServiceProvider _serviceProvider;

        public DefaultOrchardShell(
            IEnumerable<IRouteProvider> routeProviders,
            IRoutePublisher routePublisher,
            IEnumerable<IMiddlewareProvider> middlewareProviders,
            ShellSettings shellSettings,
            IServiceProvider serviceProvider) {
            _routeProviders = routeProviders;
            _routePublisher = routePublisher;
            _middlewareProviders = middlewareProviders;
            _shellSettings = shellSettings;
            _serviceProvider = serviceProvider;
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

            var allRoutes = new List<RouteDescriptor>();
            allRoutes.AddRange(_routeProviders.SelectMany(provider => provider.GetRoutes()));

            _routePublisher.Publish(allRoutes, pipeline);
        }

        public void Terminate() {
        }
    }
}