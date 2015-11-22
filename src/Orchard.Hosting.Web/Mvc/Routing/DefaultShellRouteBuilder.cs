using System;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc.Infrastructure;
using Microsoft.AspNet.Routing;

namespace Orchard.Hosting.Mvc.Routing
{
    public class DefaultShellRouteBuilder : IRouteBuilder
    {
        public DefaultShellRouteBuilder(IServiceProvider serviceProvider)
        {
            Routes = new List<IRouter>();
            DefaultHandler = new MvcRouteHandler();
            ServiceProvider = serviceProvider;
        }

        public IRouter DefaultHandler { get; set; }

        public IServiceProvider ServiceProvider { get; set; }

        public IList<IRouter> Routes
        {
            get;
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