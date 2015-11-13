using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using System;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Orchard.Environment.Shell;
using Orchard.Hosting.ShellBuilders;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Orchard.Environment.Shell.Models;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Events;

namespace Orchard.Hosting
{
    public class OrchardContainerMiddleware {
        private readonly RequestDelegate _next;
        private readonly IOrchardHost _orchardHost;
        private readonly IRunningShellTable _runningShellTable;
        private readonly ILogger _logger;
        private readonly HashSet<string> _activatedShells = new HashSet<string>();

        //private readonly IDictionary<string, ShellContext> _shellContexts = new ConcurrentDictionary<string, ShellContext>();

        public OrchardContainerMiddleware(
            RequestDelegate next,
            IOrchardHost orchardHost,
            IRunningShellTable runningShellTable,
            ILoggerFactory loggerFactory) {
            _next = next;
            _orchardHost = orchardHost;
            _runningShellTable = runningShellTable;
            _logger = loggerFactory.CreateLogger<OrchardContainerMiddleware>();
        }

        public async Task Invoke(HttpContext httpContext) {
            var sw = Stopwatch.StartNew();

            // Ensure all shell contexts are loaded and available.
            _orchardHost.Initialize();

            var shellSetting = _runningShellTable.Match(httpContext);

            if (shellSetting != null) {
                ShellContext shellContext = _orchardHost.GetShellContext(shellSetting);

                httpContext.ApplicationServices = shellContext.LifetimeScope;

                var scope = shellContext.LifetimeScope.GetRequiredService<IServiceScopeFactory>().CreateScope();

                using (scope)
                {
                    //(httpContext.RequestServices as IDisposable)?.Dispose();
                    httpContext.RequestServices = scope.ServiceProvider;

                    // We need to activate once the scope is set otherwise Scoped services
                    // won't be disposed. This is the case with Automatic Data Migrations for
                    // instance as it requires an ISession
                    if (shellContext.Shell == null)
                    {
                        shellContext.Shell = httpContext.RequestServices.GetRequiredService<IOrchardShell>();
                        shellContext.Shell.Activate();

                        var eventBus = scope.ServiceProvider.GetService<IEventBus>();
                        await eventBus.NotifyAsync<IOrchardShellEvents>(x => x.ActivatedAsync());
                    }

                    await _next.Invoke(httpContext);
                }
            }
            else {
                _logger.LogError("Tenant not found");
                throw new Exception("Tenant not found");
            }
            _logger.LogVerbose("Request took {0}ms", sw.ElapsedMilliseconds);
        }
    }
}