using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Routing;
using Microsoft.Framework.DependencyInjection;
using System;
using System.Threading.Tasks;
using OrchardVNext.Configuration.Environment;

namespace OrchardVNext.Mvc.Routes {
    public class TenantRoute : IRouter {
        private readonly IRouter _target;
        private readonly string _urlHost;
        private readonly RequestDelegate _pipeline;

        public TenantRoute(IRouter target, string urlHost, RequestDelegate pipeline) {
            _target = target;
            _urlHost = urlHost;
            _pipeline = pipeline;
        }

        public async Task RouteAsync(RouteContext context) {
            if (IsValidRequest(context)) {
                context.HttpContext.Items["orchard.Handler"] = new Func<Task>(async () => {
                    try {
                        await _target.RouteAsync(context);
                    }
                    catch (Exception e) {
                        Logger.Error(e, e.Message);
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