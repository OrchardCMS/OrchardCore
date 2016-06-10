using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Orchard.Routes
{
    public class PrefixedRouteBuilder : IRouteBuilder
    {
        private readonly IRouteBuilder _baseRouteBuilder;
        private readonly List<IRouter> _routes = new List<IRouter>();

        public PrefixedRouteBuilder(IRouteBuilder baseRouteBuilder)
        {
            _baseRouteBuilder = baseRouteBuilder;
        }
        public IApplicationBuilder ApplicationBuilder
        {
            get
            {
                return _baseRouteBuilder.ApplicationBuilder;
            }
        }

        public IRouter DefaultHandler
        {
            get
            {
                return _baseRouteBuilder.DefaultHandler;
            }

            set
            {
                _baseRouteBuilder.DefaultHandler = value;
            }
        }

        public IList<IRouter> Routes
        {
            get
            {
                return _routes;
            }
        }

        public IServiceProvider ServiceProvider
        {
            get
            {
                return _baseRouteBuilder.ServiceProvider;
            }
        }

        public IRouter Build()
        {
            return null;
        }
    }
}
