using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DeferredTasks;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Modules
{
    /// <summary>
    /// This middleware replaces the default service provider by the one for the current tenant
    /// </summary>
    public class ModularTenantContainerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IShellHost _orchardHost;
        private readonly IRunningShellTable _runningShellTable;
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores = new ConcurrentDictionary<string, SemaphoreSlim>();

        public ModularTenantContainerMiddleware(
            RequestDelegate next,
            IShellHost orchardHost,
            IRunningShellTable runningShellTable)
        {
            _next = next;
            _orchardHost = orchardHost;
            _runningShellTable = runningShellTable;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            // Ensure all ShellContext are loaded and available.
            _orchardHost.Initialize();

            var shellSettings = _runningShellTable.Match(httpContext);

            // Register the shell settings as a custom feature.
            httpContext.Features.Set(shellSettings);

            // We only serve the next request if the tenant has been resolved.
            if (shellSettings != null)
            {
                var shellContext = _orchardHost.GetOrCreateShellContext(shellSettings);

                var hasPendingTasks = false;
                using (var scope = shellContext.EnterServiceScope())
                {
                    if (!shellContext.IsActivated)
                    {
                        var semaphore = _semaphores.GetOrAdd(shellSettings.Name, (name) => new SemaphoreSlim(1));

                        await semaphore.WaitAsync();

                        try
                        {
                            // The tenant gets activated here
                            if (!shellContext.IsActivated)
                            {
                                var tenantEvents = scope.ServiceProvider.GetServices<IModularTenantEvents>();

                                foreach (var tenantEvent in tenantEvents)
                                {
                                    await tenantEvent.ActivatingAsync();
                                }

                                httpContext.Items["BuildPipeline"] = true;

                                foreach (var tenantEvent in tenantEvents.Reverse())
                                {
                                    await tenantEvent.ActivatedAsync();
                                }

                                shellContext.IsActivated = true;
                            }
                        }
                        finally
                        {                            
                            semaphore.Release();
                            _semaphores.TryRemove(shellSettings.Name, out semaphore);
                        }
                    }
                    
                    await _next.Invoke(httpContext);
                    var deferredTaskEngine = scope.ServiceProvider.GetService<IDeferredTaskEngine>();
                    hasPendingTasks = deferredTaskEngine.HasPendingTasks;
                }

                // Create a new scope only if there are pending tasks
                if (hasPendingTasks)
                {
                    shellContext = _orchardHost.GetOrCreateShellContext(shellSettings);

                    using (var scope = shellContext.EnterServiceScope())
                    {
                        var deferredTaskEngine = scope.ServiceProvider.GetService<IDeferredTaskEngine>();
                        var context = new DeferredTaskContext(scope.ServiceProvider);
                        await deferredTaskEngine.ExecuteTasksAsync(context);
                    }
                }
            }
        }
    }
}
