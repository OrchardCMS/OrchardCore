using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Modules
{
    /// <summary>
    /// This middleware replaces the default service provider by the one for the current tenant
    /// </summary>
    public class ModularTenantContainerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IShellHost _shellHost;
        private readonly IRunningShellTable _runningShellTable;

        public ModularTenantContainerMiddleware(
            RequestDelegate next,
            IShellHost shellHost,
            IRunningShellTable runningShellTable)
        {
            _next = next;
            _shellHost = shellHost;
            _runningShellTable = runningShellTable;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            // Ensure all ShellContext are loaded and available.
            await _shellHost.InitializeAsync();

            var shellSettings = _runningShellTable.Match(httpContext);

            // We only serve the next request if the tenant has been resolved.
            if (shellSettings is not null)
            {
                if (shellSettings.IsInitializing())
                {
                    httpContext.Response.Headers.Append(HeaderNames.RetryAfter, "10");
                    httpContext.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                    await httpContext.Response.WriteAsync("The requested tenant is currently initializing.");
                    return;
                }

                // Makes 'RequestServices' aware of the current 'ShellScope'.
                httpContext.UseShellScopeServices();

                var shellScope = await _shellHost.GetScopeAsync(shellSettings);

                // Holds the 'ShellContext' for the full request.
                httpContext.Features.Set(new ShellContextFeature
                {
                    ShellContext = shellScope.ShellContext,
                    OriginalPath = httpContext.Request.Path,
                    OriginalPathBase = httpContext.Request.PathBase
                });

                await shellScope.UsingAsync(async scope =>
                {
                    await _next.Invoke(httpContext);

                    var feature = httpContext.Features.Get<IExceptionHandlerFeature>();
                    if (feature?.Error is not null)
                    {
                        await scope.HandleExceptionAsync(feature.Error);
                    }
                });
            }
        }
    }
}
