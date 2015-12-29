using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Shell;
using Orchard.Hosting.ShellBuilders;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Events;

namespace Orchard.Hosting
{
    /// <summary>
    /// This middleware replaces the default service provider by the one for the current tenant
    /// </summary>
    public class OrchardContainerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IOrchardHost _orchardHost;
        private readonly IRunningShellTable _runningShellTable;
        private readonly ILogger _logger;

        public OrchardContainerMiddleware(
            RequestDelegate next,
            IOrchardHost orchardHost,
            IRunningShellTable runningShellTable,
            ILogger<OrchardContainerMiddleware> logger)
        {
            _next = next;
            _orchardHost = orchardHost;
            _runningShellTable = runningShellTable;
            _logger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            // Ensure all ShellContext are loaded and available.
            _orchardHost.Initialize();

            var shellSetting = _runningShellTable.Match(httpContext);

            // We only serve the next request if the tenant has been resolved.
            if (shellSetting != null)
            {
                ShellContext shellContext = _orchardHost.GetShellContext(shellSetting);
                httpContext.ApplicationServices = shellContext.ServiceProvider;

                var scope = shellContext.ServiceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();

                using (scope)
                {
                    httpContext.RequestServices = scope.ServiceProvider;

                    if (!shellContext.IsActivated)
                    {
                        lock (shellSetting)
                        {
                            // The tenant gets activated here
                            if (!shellContext.IsActivated)
                            {
                                var eventBus = scope.ServiceProvider.GetService<IEventBus>();
                                eventBus.NotifyAsync<IOrchardShellEvents>(x => x.ActivatingAsync()).Wait();
                                eventBus.NotifyAsync<IOrchardShellEvents>(x => x.ActivatedAsync()).Wait();

                                shellContext.IsActivated = true;
                            }
                        }
                    }

                    await _next.Invoke(httpContext);
                }
            }
        }
    }
}