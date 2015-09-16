using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using System;
using Orchard.Configuration.Environment;
using Microsoft.Framework.Logging;
using System.Diagnostics;

namespace Orchard.Hosting {
    public class OrchardContainerMiddleware {
        private readonly RequestDelegate _next;
        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly IOrchardHost _orchardHost;
        private readonly ILogger _logger;

        public OrchardContainerMiddleware(
            RequestDelegate next,
            IShellSettingsManager shellSettingsManager,
            IOrchardHost orchardHost,
            ILoggerFactory loggerFactory) {
            _next = next;
            _shellSettingsManager = shellSettingsManager;
            _orchardHost = orchardHost;
            _logger = loggerFactory.CreateLogger<OrchardContainerMiddleware>();
        }

        public async Task Invoke(HttpContext httpContext) {
            var sw = Stopwatch.StartNew();
            var shellSetting = GetSettings(httpContext.Request.Host.Value);

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

        private ShellSettings GetSettings(string requestHost) {
            var shellSettings = _shellSettingsManager.LoadSettings();

            if (!shellSettings.Any()) {
                return ShellHelper.BuildDefaultUninitializedShell;
            }

            return shellSettings
                .SingleOrDefault(x => x.RequestUrlHost == requestHost);
        }
    }
}