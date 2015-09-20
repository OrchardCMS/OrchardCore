using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Framework.DependencyInjection;
using Orchard.Environment.Shell;

namespace Orchard.Hosting {
    public class OrchardShellHostMiddleware {
        private readonly RequestDelegate _next;
        private readonly IOrchardShellHost _orchardShellHost;

        public OrchardShellHostMiddleware(
            RequestDelegate next,
            IOrchardShellHost orchardShellHost) {

            _next = next;
            _orchardShellHost = orchardShellHost;
        }

        public async Task Invoke(HttpContext httpContext, ShellSettings shellSettings) {
            _orchardShellHost.BeginRequest(shellSettings);

            httpContext.Response.Headers.Append("X-Generator", "Orchard");

            await _next.Invoke(httpContext);

            _orchardShellHost.EndRequest(shellSettings);
        }
    }
}