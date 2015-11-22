using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using System;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
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
            ILoggerFactory loggerFactory)
        {
            _next = next;
            _orchardHost = orchardHost;
            _runningShellTable = runningShellTable;
            _logger = loggerFactory.CreateLogger<OrchardContainerMiddleware>();
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var sw = Stopwatch.StartNew();

            // Ensure all ShellContext are loaded and available.
            _orchardHost.Initialize();

            var shellSetting = _runningShellTable.Match(httpContext);

            if (shellSetting != null)
            {
                ShellContext shellContext = _orchardHost.GetShellContext(shellSetting);
                httpContext.Items["ShellSettings"] = shellSetting;
                httpContext.ApplicationServices = shellContext.ServiceProvider;

                var scope = shellContext.ServiceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();

                using (scope)
                {
                    httpContext.RequestServices = scope.ServiceProvider;

                    if (!shellContext.IsActived)
                    {
                        var eventBus = scope.ServiceProvider.GetService<IEventBus>();
                        await eventBus.NotifyAsync<IOrchardShellEvents>(x => x.ActivatingAsync());
                        await eventBus.NotifyAsync<IOrchardShellEvents>(x => x.ActivatedAsync());

                        shellContext.IsActived = true;
                    }

                    await _next.Invoke(httpContext);
                }
            }
            else
            {
                _logger.LogError("Tenant not found");
                throw new Exception("Tenant not found");
            }
            _logger.LogVerbose("Request took {0}ms", sw.ElapsedMilliseconds);
        }
    }
}