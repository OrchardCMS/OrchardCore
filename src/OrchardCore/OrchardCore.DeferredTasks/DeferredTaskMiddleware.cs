using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell;

namespace OrchardCore.DeferredTasks
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
                var deferredTaskEngine = httpContext.RequestServices.GetService<IDeferredTaskEngine>();

                // Create a new scope only if there are pending tasks
                if (deferredTaskEngine != null && deferredTaskEngine.HasPendingTasks)
                {
                    // Dispose the scoped services for the current request, and create a new one
                    (httpContext.RequestServices as IDisposable).Dispose();

                    var shellContext = _orchardHost.GetOrCreateShellContext(shellSettings);

                    if (!shellContext.Released)
                    {
                        using (var scope = shellContext.EnterServiceScope())
                        {
                            var context = new DeferredTaskContext(scope.ServiceProvider);
                            await deferredTaskEngine.ExecuteTasksAsync(context);
                        }
                    }
                }
            }
        }
    }
}