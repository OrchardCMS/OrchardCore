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

namespace Orchard.Hosting
{
    public class OrchardContainerMiddleware {
        private readonly RequestDelegate _next;
        private readonly IOrchardHost _orchardHost;
        private readonly IRunningShellTable _runningShellTable;
        private readonly ILogger _logger;

        private readonly IDictionary<string, ShellContext> _shellContexts = new ConcurrentDictionary<string, ShellContext>();

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
            var shellSetting = _runningShellTable.Match(httpContext);

            if (shellSetting != null) {
                ShellContext shellContext;
                if(!_shellContexts.TryGetValue(shellSetting.Name, out shellContext))
                {
                    // Only lock on the requested Shell
                    lock(shellSetting)
                    {
                        if (!_shellContexts.ContainsKey(shellSetting.Name))
                        {
                            shellContext = _orchardHost.CreateShellContext(shellSetting);
                            shellContext.Shell.Activate();

                            if (shellSetting.State == TenantState.Initializing ||
                                shellSetting.State == TenantState.Running )
                            {
                                _shellContexts.Add(shellSetting.Name, shellContext);
                            }
                        }
                    }
                }

                httpContext.RequestServices = shellContext.LifetimeScope;
                await _next.Invoke(httpContext);
            }
            else {
                _logger.LogError("Tenant not found");
                throw new Exception("Tenant not found");
            }
            _logger.LogVerbose("Request took {0}ms", sw.ElapsedMilliseconds);
        }
    }
}