using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Environment.Shell;
using Orchard.Hosting;
using Orchard.Hosting.ShellBuilders;

namespace Orchard.DeferredTasks
{
    /// <summary>
    /// Executes any deferred tasks when the request is terminated.
    /// </summary>
    public class DeferredTaskMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IOrchardHost _orchardHost;

        public DeferredTaskMiddleware(RequestDelegate next, IOrchardHost orchardHost)
        {
            _next = next;
            _orchardHost = orchardHost;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            await _next.Invoke(httpContext);

            // Register the shell settings as a custom feature.
            var shellSettings = httpContext.Features[typeof(ShellSettings)] as ShellSettings;

            // We only serve the next request if the tenant has been resolved.
            if (shellSettings != null)
            {
                ShellContext shellContext = _orchardHost.GetOrCreateShellContext(shellSettings);

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
