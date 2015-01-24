using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Framework.DependencyInjection;
using OrchardVNext.Environment.Configuration;

namespace OrchardVNext.Environment {
    public class OrchardShellHostMiddleware {
        private readonly RequestDelegate _next;
        private readonly IOrchardShellHost _orchardHost;

        public OrchardShellHostMiddleware(
            RequestDelegate next,
            IOrchardShellHost orchardHost) {

            _next = next;
            _orchardHost = orchardHost;
        }

        public async Task Invoke(HttpContext httpContext) {
            var shellSettings = httpContext.RequestServices.GetService<ShellSettings>();
            _orchardHost.BeginRequest(shellSettings);
            httpContext.Response.Headers.Append("X-Generator", "Orchard");
            await _next.Invoke(httpContext).ContinueWith(x => {
                _orchardHost.EndRequest(shellSettings);
            });
        }
    }
}