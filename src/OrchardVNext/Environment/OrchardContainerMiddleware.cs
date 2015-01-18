using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using OrchardVNext.Environment.Configuration;
using OrchardVNext.Environment.ShellBuilders;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OrchardVNext.Environment {
    public class OrchardContainerMiddleware {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;
        private readonly IShellSettingsManager _shellSettingsManager;
        private readonly IShellContextFactory _shellContextFactory;


        public OrchardContainerMiddleware(
            RequestDelegate next,
            IServiceProvider serviceProvider,
            IShellSettingsManager shellSettingsManager,
            IShellContextFactory shellContextFactory) {
            _next = next;
            _serviceProvider = serviceProvider;
            _shellSettingsManager = shellSettingsManager;
            _shellContextFactory = shellContextFactory;
        }

        public async Task Invoke(HttpContext httpContext) {
            var currentApplicationServices = httpContext.ApplicationServices;
            var currentRequestServices = httpContext.RequestServices;

            var shellSettings = _shellSettingsManager.LoadSettings();
            var shellSetting = shellSettings
                .SingleOrDefault(x => x.RequestUrlPrefix == httpContext.Request.Host.Value);

            if (shellSetting != null) {
                using (var shell = _shellContextFactory.CreateShellContext(shellSetting)) {
                    httpContext.RequestServices = shell.LifetimeScope;

                    shell.Shell.Activate();
                    await _next.Invoke(httpContext);
                }
            }
            else {
                await _next.Invoke(httpContext);
            }
        }
    }
}