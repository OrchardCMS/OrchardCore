using Microsoft.AspNet.Http;
using Microsoft.AspNet.Routing;
using OrchardVNext.Environment.Configuration;
using OrchardVNext.Mvc.Routes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardVNext.Environment {
    public interface IOrchardShell {
        void Activate();
        void Terminate();
    }

    public class DefaultOrchardShell : IOrchardShell {
        private readonly IEnumerable<IRouteProvider> _routeProviders;
        private readonly IRoutePublisher _routePublisher;

        public DefaultOrchardShell(
            IEnumerable<IRouteProvider> routeProviders,
            IRoutePublisher routePublisher) {
            _routeProviders = routeProviders;
            _routePublisher = routePublisher;
        }

        public void Activate() {
            var allRoutes = new List<RouteDescriptor>();
            allRoutes.AddRange(_routeProviders.SelectMany(provider => provider.GetRoutes()));

            _routePublisher.Publish(allRoutes);

            //var endpoint = new DelegateRouteEndpoint(async (context) =>
            //                                        await context
            //                                                .HttpContext
            //                                                .Response
            //                                                .WriteAsync("Hello, World! from " + _shellSettings.Name));

            //_routeBuilder.AddPrefixRoute(_shellSettings.RequestUrlPrefix, "hello/world", endpoint);
        }

        public void Terminate() {
        }
    }

    //var endpoint = new DelegateRouteEndpoint(async (context) =>
    //                                        await context
    //                                                .HttpContext
    //                                                .Response
    //                                                .WriteAsync("Hello, World! from " + _shellSettings.Name));

    //_routeBuilder.AddPrefixRoute(_shellSettings.RequestUrlPrefix, "hello/world", endpoint);






}