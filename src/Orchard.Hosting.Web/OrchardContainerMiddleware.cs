using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using System;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Orchard.Environment.Shell;

namespace Orchard.Hosting {
    public class OrchardContainerMiddleware {
        private readonly RequestDelegate _next;
        private readonly IOrchardHost _orchardHost;
        private readonly IRunningShellTable _runningShellTable;
        private readonly ILogger _logger;

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
                using (var shell = _orchardHost.CreateShellContext(shellSetting)) {
                    httpContext.RequestServices = shell.LifetimeScope;

                    shell.Shell.Activate();
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