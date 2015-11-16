using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Shell;
using Microsoft.AspNet.Http;

namespace Orchard.Hosting.Web.Routing.Routes
{
    public class TenantRoute : IRouter
    {
        private readonly IRouter _target;
        private readonly RequestDelegate _pipeline;
        private readonly ShellSettings _shellSettings;

        public TenantRoute(
            ShellSettings shellSettings,
            IRouter target,
            RequestDelegate pipeline)
        {
            _target = target;
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

                    if (_pipeline != null)
                    {
                        context.HttpContext.Items["orchard.Handler.Target"] = _target;
                        context.HttpContext.Items["orchard.Handler.RouteContext"] = context;

                        await _pipeline.Invoke(context.HttpContext);
                    }
                    else
                    {
                        await _target.RouteAsync(context);
                    }
                }
                catch (Exception ex)
                {
                    var loggerFactory = context.HttpContext.ApplicationServices.GetService<ILoggerFactory>();
                    var logger = loggerFactory.CreateLogger<TenantRoute>();
                    logger.LogError("Error occured serving tenant route", ex);
                    throw;
                }
            }
        }

        private bool IsValidRequest(HttpContext httpContext)
        {
            return httpContext.Items["ShellSettings"] == _shellSettings;
        }

        public VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            if (IsValidRequest(context.Context))
            {
                return _target.GetVirtualPath(context);
            }

            return null;
        }
    }
}