using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Shell;

namespace Orchard.Hosting.Web.Routing.Routes {
    public class TenantRoute : IRouter {
        private readonly IRouter _target;
        private readonly string _urlHost;
        private readonly RequestDelegate _pipeline;

        public TenantRoute(IRouter target, 
            string urlHost, 
            RequestDelegate pipeline) {
            _target = target;
            _urlHost = urlHost;
            _pipeline = pipeline;
        }

        public async Task RouteAsync(RouteContext context) {
            if (IsValidRequest(context)) {
                context.HttpContext.Items["orchard.Handler"] = new Func<Task>(async () => {
                    var loggerFactory = context.HttpContext.ApplicationServices.GetService<ILoggerFactory>();
                    var logger = loggerFactory.CreateLogger<TenantRoute>();
                    
                    try {
                        await _target.RouteAsync(context);
                    }
                    catch (Exception ex) {

                        logger.LogError("Error occured serving tenant route", ex);
                        throw;
                    }
                });

                await _pipeline.Invoke(context.HttpContext);
            }
        }

        private bool IsValidRequest(RouteContext context) {
            if (context.HttpContext.Request.Host.Value == _urlHost) {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(_urlHost))
                return false;

            // TODO: ngm: revise this.
            // This is normally when requesting the default tenant.
            var shellSettings = context.HttpContext.RequestServices.GetService<ShellSettings>();

            if (shellSettings.RequestUrlHost == _urlHost) {
                return true;
            }

            return false;
        }

        public VirtualPathData GetVirtualPath(VirtualPathContext context) {
            return null;
        }
    }
}