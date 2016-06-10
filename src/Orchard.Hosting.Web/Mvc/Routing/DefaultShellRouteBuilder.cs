using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Routing;
using Orchard.Environment.Shell;
using Orchard.Hosting.Web.Routing.Routes;

namespace Orchard.Hosting.Mvc.Routing
{
    /// <summary>
    /// Implements <see cref="IRouteBuilder"/> by indexing routes by tenant.
    /// TODO: OrchardRouteMiddleware seems to already contain this logic
    /// </summary>
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
            var shellRoute = new ShellRoute();

            foreach (var route in Routes.OfType<TenantRoute>())
            {
                shellRoute.TenantRoutes.Add(route.ShellSettings.Name, route);
            }

            return shellRoute;
        }

        private class ShellRoute : IRouter
        {
            public Dictionary<string, TenantRoute> TenantRoutes { get; } = new Dictionary<string, TenantRoute>();

            public VirtualPathData GetVirtualPath(VirtualPathContext context)
            {
                var shellSettings = context.HttpContext.RequestServices.GetService<ShellSettings>();

                if(shellSettings == null)
                {
                    return null;
                }

                TenantRoute route;
                if (TenantRoutes.TryGetValue(shellSettings.Name, out route))
                {
                    return route.GetVirtualPath(context);
                }

                return null;
            }

            public Task RouteAsync(RouteContext context)
            {
                var shellSettings = context.HttpContext.RequestServices.GetService<ShellSettings>();

                if (shellSettings == null)
                {
                    return null;
                }

                TenantRoute route;
                if (TenantRoutes.TryGetValue(shellSettings.Name, out route))
                {
                    return route.RouteAsync(context);
                }

                return Task.CompletedTask;
            }
        }
    }
}