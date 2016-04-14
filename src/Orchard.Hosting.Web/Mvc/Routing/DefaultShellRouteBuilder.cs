using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Routing;

namespace Orchard.Hosting.Mvc.Routing
{
    public class DefaultShellRouteBuilder : IRouteBuilder
    {
        public DefaultShellRouteBuilder(IServiceProvider serviceProvider)
            : this(serviceProvider, defaultHandler: new MvcRouteHandler())
        {
        }

        public DefaultShellRouteBuilder(IServiceProvider serviceProvider, IRouter defaultHandler)
        {
            DefaultHandler = defaultHandler;
            ServiceProvider = serviceProvider;
            Routes = new List<IRouter>();
        }

        public IRouter DefaultHandler { get; set; }
        public IServiceProvider ServiceProvider { get; }
        public IList<IRouter> Routes { get; }

        public IApplicationBuilder ApplicationBuilder
        {
            get
            {
                throw new NotImplementedException();
            }
        }

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