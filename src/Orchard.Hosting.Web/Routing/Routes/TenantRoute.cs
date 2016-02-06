using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Shell;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orchard.Hosting.Web.Routing.Routes
{
    public class TenantRoute : IRouter
    {
        private readonly RequestDelegate _pipeline;
        private readonly ShellSettings _shellSettings;
        private readonly IEnumerable<IRouter> _routes;

        public TenantRoute(
            ShellSettings shellSettings,
            IEnumerable<IRouter> routes,
            RequestDelegate pipeline)
        {
            _routes = routes;
            _shellSettings = shellSettings;
            _pipeline = pipeline;
        }

        public async Task RouteAsync(RouteContext context)
        {
            if (IsValidRequest(context.HttpContext))
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
                    var logger = context.HttpContext.ApplicationServices.GetService<ILogger<TenantRoute>>();
                    logger.LogError("Error occured serving tenant route", ex);
                    throw;
                }
            }
        }

        /// <summary>
        /// Returns <c>True</c> if this tenant route matches the current tenant
        /// </summary>
        private bool IsValidRequest(HttpContext httpContext)
        {
            return httpContext.RequestServices.GetService<ShellSettings>() == _shellSettings;
        }

        public VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            if (IsValidRequest(context.Context))
            {
                foreach (var route in _routes)
                {
                    var result = route.GetVirtualPath(context);
                    if(result != null)
                    {
                        return result;
                    }
                }
            }

            return null;
        }
    }
}