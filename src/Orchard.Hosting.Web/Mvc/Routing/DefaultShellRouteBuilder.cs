using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Routing;

namespace Orchard.Hosting.Mvc.Routing
{
    public class DefaultShellRouteBuilder : IRouteBuilder
    {
        public DefaultShellRouteBuilder(IApplicationBuilder applicationBuilder)
            : this(applicationBuilder, defaultHandler: new MvcRouteHandler())
        {
        }

        public DefaultShellRouteBuilder(IApplicationBuilder applicationBuilder, IRouter defaultHandler)
        {
            if (applicationBuilder == null)
            {
                throw new ArgumentNullException(nameof(applicationBuilder));
            }

            ApplicationBuilder = applicationBuilder;
            DefaultHandler = defaultHandler;
            ServiceProvider = applicationBuilder.ApplicationServices;
            Routes = new List<IRouter>();
        }

        public IApplicationBuilder ApplicationBuilder { get; }
        public IRouter DefaultHandler { get; set; }
        public IServiceProvider ServiceProvider { get; }
        public IList<IRouter> Routes { get; }

        public IRouter Build()
        {
            var routeCollection = new RouteCollection();

            foreach (var route in Routes)
            {
                routeCollection.Add(route);
            }

            return routeCollection;
        }
    }
}