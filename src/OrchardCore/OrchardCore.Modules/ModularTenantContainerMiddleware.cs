using System.Linq;
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

                IDeferredTaskEngine deferredTaskEngine;
                using (var scope = shellContext.EnterServiceScope())
                {
                    if (!shellContext.IsActivated || shellContext.IsActivating)
                    {
                        await shellContext.SemaphoreSlim.WaitAsync();

                        try
                        {
                            // The tenant gets activated here
                            if (!shellContext.IsActivated)
                            {
                                shellContext.IsActivating = true;

                                var tenantEvents = scope.ServiceProvider.GetServices<IModularTenantEvents>();

                                foreach (var tenantEvent in tenantEvents)
                                {
                                    tenantEvent.ActivatingAsync().Wait();
                                }

                                httpContext.Items["BuildPipeline"] = true;
                                shellContext.IsActivated = true;

                                foreach (var tenantEvent in tenantEvents.Reverse())
                                {
                                    tenantEvent.ActivatedAsync().Wait();
                                }

                                shellContext.IsActivating = false;
                            }
                        }
                        finally
                        {
                            shellContext.SemaphoreSlim.Release();
                        }
                    }

                    shellContext.RequestStarted();

                    try
                    {
                        await _next.Invoke(httpContext);
                        deferredTaskEngine = scope.ServiceProvider.GetService<IDeferredTaskEngine>();
                    }
                    finally
                    {
                        shellContext.RequestEnded();

                        // Call all terminating events before releasing the shell context
                        if (shellContext.CanTerminate)
                        {
                            var tenantEvents = scope.ServiceProvider.GetServices<IModularTenantEvents>();

                            foreach (var tenantEvent in tenantEvents)
                            {
                                await tenantEvent.TerminatingAsync();
                            }

                            foreach (var tenantEvent in tenantEvents.Reverse())
                            {
                                await tenantEvent.TerminatedAsync();
                            }
                        }
                    }
                }

                // Create a new scope only if there are pending tasks
                if (deferredTaskEngine != null && deferredTaskEngine.HasPendingTasks)
                {
                    shellContext = _orchardHost.GetOrCreateShellContext(shellSettings);

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