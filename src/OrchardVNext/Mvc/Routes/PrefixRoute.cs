using Microsoft.AspNet.Routing;
using System.Threading.Tasks;

namespace OrchardVNext.Mvc.Routes {
    public class TenantRoute : IRouter {
        private readonly IRouter _target;
        private readonly string _urlHost;

        public TenantRoute(IRouter target, string urlHost) {
            _target = target;
            _urlHost = urlHost;
        }

        public async Task RouteAsync(RouteContext context) {
            if (context.HttpContext.Request.Host.Value == _urlHost) {
                await _target.RouteAsync(context);
            }
        }

        public string GetVirtualPath(VirtualPathContext context) {
            return null;
        }
    }
}