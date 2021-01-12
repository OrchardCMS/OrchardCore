using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Modules
{
    /// <summary>
    /// This middleware replaces the default service container by the one for the current tenant.
    /// </summary>
    public class ModularTenantContainerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IShellHost _shellHost;

        public ModularTenantContainerMiddleware(
            RequestDelegate next,
            IShellHost shellHost)
        {
            _next = next;
            _shellHost = shellHost;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            // We only call the next request delegate if the tenant has been resolved.
            var shellSettingsFeature = httpContext.Features.Get<ShellSettingsFeature>();
            if (shellSettingsFeature != null)
            {
                // Makes 'RequestServices' aware of the current 'ShellScope'.
                httpContext.UseShellScopeServices();

                var shellScope = await _shellHost.GetScopeAsync(shellSettingsFeature.ShellSettings);

                // Holds the 'ShellContext' for the full request.
                httpContext.Features.Set(new ShellContextFeature
                {
                    ShellContext = shellScope.ShellContext,
                    OriginalPath = shellSettingsFeature.OriginalPath,
                    OriginalPathBase = shellSettingsFeature.OriginalPathBase
                });

                await shellScope.UsingAsync(scope => _next.Invoke(httpContext));
            }
        }
    }
}
