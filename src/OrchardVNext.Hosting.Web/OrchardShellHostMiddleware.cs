using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Framework.DependencyInjection;
using OrchardVNext.Configuration.Environment;

namespace OrchardVNext.Hosting {
    public class OrchardShellHostMiddleware {
        private readonly RequestDelegate _next;
        private readonly IShellHost _shellHost;

        public OrchardShellHostMiddleware(
            RequestDelegate next,
            IShellHost shellHost) {

            _next = next;
            _shellHost = shellHost;
        }

        public async Task Invoke(HttpContext httpContext) {
            var shellSettings = httpContext.RequestServices.GetService<ShellSettings>();
            _shellHost.BeginRequest(shellSettings);

            httpContext.Response.Headers.Append("X-Generator", "Orchard");

            await _next.Invoke(httpContext).ContinueWith(x => {
                _shellHost.EndRequest(shellSettings);
            });
        }
    }
}