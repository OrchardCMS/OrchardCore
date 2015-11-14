using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Shell;

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
            if (IsValidRequest(context))
            {
                context.HttpContext.Items["orchard.Handler"] = new Func<Task>(async () =>
                {
                    try
                    {
                        await _target.RouteAsync(context);
                    }
                    catch (Exception ex)
                    {
                        var loggerFactory = context.HttpContext.ApplicationServices.GetService<ILoggerFactory>();
                        var logger = loggerFactory.CreateLogger<TenantRoute>();
                        logger.LogError("Error occured serving tenant route", ex);
                        throw;
                    }
                });

                await _pipeline.Invoke(context.HttpContext);
            }
        }

        private bool IsValidRequest(RouteContext context)
        {
            return true;

            //if (String.Equals(
            //    context.HttpContext.Request.Host.Value,
            //    _shellSettings.RequestUrlHost,
            //    StringComparison.OrdinalIgnoreCase))
            //{
            //    return true;
            //}

            //if (!string.IsNullOrWhiteSpace(_shellSettings.RequestUrlHost))
            //{
            //    return false;
            //}

            //return false;
        }

        public VirtualPathData GetVirtualPath(VirtualPathContext context)
        {
            return _target.GetVirtualPath(context);
        }
    }
}