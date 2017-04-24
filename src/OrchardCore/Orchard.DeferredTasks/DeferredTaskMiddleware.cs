using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Environment.Shell;
using Orchard.Hosting;

namespace Orchard.DeferredTasks
{
    /// <summary>
    /// Executes any deferred tasks when the request is terminated.
    /// </summary>
    public class DeferredTaskMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IShellHost _orchardHost;

        public DeferredTaskMiddleware(RequestDelegate next, IShellHost orchardHost)
        {
            _next = next;
            _orchardHost = orchardHost;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            await _next.Invoke(httpContext);

            // Register the shell settings as a custom feature.
            var shellSettings = httpContext.Features.Get<ShellSettings>();

            // We only serve the next request if the tenant has been resolved.
            if (shellSettings != null)
            {
                var shellContext = _orchardHost.GetOrCreateShellContext(shellSettings);

                using (var scope = shellContext.CreateServiceScope())
                {
                    var deferredTaskEngine = scope.ServiceProvider.GetService<IDeferredTaskEngine>();

                    if (deferredTaskEngine != null && deferredTaskEngine.HasPendingTasks)
                    {
                        var context = new DeferredTaskContext(scope.ServiceProvider);
                        await deferredTaskEngine.ExecuteTasksAsync(context);
                    }
                }
            }
        }
    }
}
