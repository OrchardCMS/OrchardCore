using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Shell;

namespace Orchard.Hosting.Web.Routing.Routes
{
    public class TenantRoute : IRouter
    {
        private readonly RequestDelegate _pipeline;
        private readonly IEnumerable<IRouter> _routes;

        public TenantRoute(
            ShellSettings shellSettings,
            IEnumerable<IRouter> routes,
            RequestDelegate pipeline)
        {
            _routes = routes;
            ShellSettings = shellSettings;
            _pipeline = pipeline;
        }

        public ShellSettings ShellSettings { get; }

        public async Task RouteAsync(RouteContext context)
        {
            try
            {
                // Store the requested targetted action so that the OrchardMiddleware
                // can continue with it once the tenant pipeline has been executed

                context.HttpContext.Items["orchard.middleware.context"] = context;
                context.HttpContext.Items["orchard.middleware.routes"] = _routes;

                await _pipeline.Invoke(context.HttpContext);
            }
            catch (Exception ex)
            {
                var logger = context.HttpContext.RequestServices.GetService<ILogger<TenantRoute>>();
                logger.LogError("Error occured serving tenant route", ex);
                throw;
            }
        }

        public VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            foreach (var route in _routes)
            {
                var result = route.GetVirtualPath(context);
                if(result != null)
                {
                    return result;
                }
            }

            return null;
        }
    }
}