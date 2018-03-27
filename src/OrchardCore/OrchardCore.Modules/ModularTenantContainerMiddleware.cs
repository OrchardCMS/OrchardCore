using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
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

            var shellSetting = _runningShellTable.Match(httpContext);

            // Register the shell settings as a custom feature.
            httpContext.Features.Set(shellSetting);

            var disposeContext = false;

            // We only serve the next request if the tenant has been resolved.
            if (shellSetting != null)
            {
                var shellContext = _orchardHost.GetOrCreateShellContext(shellSetting);

                var existingRequestServices = httpContext.RequestServices;

                try
                {
                    using (var scope = shellContext.EnterServiceScope(false))
                    {
                        if (!shellContext.IsActivated)
                        {
                            lock (shellContext)
                            {
                                // The tenant gets activated here
                                if (!shellContext.IsActivated)
                                {
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
                                }
                            }
                        }

                        if (!shellContext.RequestStarted())
                        {
                            // The tenant is restarting
                            httpContext.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
                            return;
                        }

                        try
                        {
                            await _next.Invoke(httpContext);
                        }
                        finally
                        {
                            // Call all terminating events before releasing the shell context
                            if (shellContext.RequestEnded())
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

                                // We can't dispose the shell context here as we are in a 'using' block with a service scope issued from this shell.
                                // We need to dispose the shell context container after the service scope.
                                disposeContext = true;
                            }
                        }
                    }
                }
                finally
                {
                    if (disposeContext)
                    {
                        shellContext.Dispose();
                    }
                }
            }
        }
    }
}