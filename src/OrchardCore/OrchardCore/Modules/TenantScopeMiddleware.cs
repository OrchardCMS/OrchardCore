using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Modules
{
    /// <summary>
    /// This middleware creates a new scope on the service container of the current tenant.
    /// </summary>
    public class TenantScopeMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IShellHost _shellHost;

        public TenantScopeMiddleware(
            RequestDelegate next,
            IShellHost shellHost)
        {
            _next = next;
            _shellHost = shellHost;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            // Makes 'RequestServices' aware of the current 'ShellScope'.
            httpContext.UseShellScopeServices();

            var feature = httpContext.Features.Get<ShellContextFeature>();
            var shellScope = await _shellHost.GetScopeAsync(feature.ShellContext.Settings);

            // Update the feature as the scope may have been created on a different shell.
            feature.ShellContext = shellScope.ShellContext;

            await shellScope.UsingAsync(scope => _next.Invoke(httpContext));
        }
    }
}
