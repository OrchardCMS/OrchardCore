using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;

namespace OrchardCore.Modules
{
    /// <summary>
    /// This middleware replaces the default service container by the one for the current tenant.
    /// </summary>
    public class TenantContainerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IShellHost _shellHost;
        private readonly IRunningShellTable _runningShellTable;

        public TenantContainerMiddleware(
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
            if (shellSettings != null)
            {
                if (shellSettings.State == TenantState.Initializing)
                {
                    httpContext.Response.Headers.Add(HeaderNames.RetryAfter, "10");
                    httpContext.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                    await httpContext.Response.WriteAsync("The requested tenant is currently initializing.");
                    return;
                }

                var shellContext = await _shellHost.GetOrCreateShellContextAsync(shellSettings);

                // Holds the 'ShellContext' for the full request.
                httpContext.Features.Set(new ShellContextFeature
                {
                    ShellContext = shellContext,
                    OriginalPath = httpContext.Request.Path,
                    OriginalPathBase = httpContext.Request.PathBase
                });

                // Define a PathBase for the current request that is the RequestUrlPrefix.
                // This will allow any view to reference ~/ as the tenant's base url.
                // Because IIS or another middleware might have already set it, we just append the tenant prefix value.
                if (!String.IsNullOrEmpty(shellContext.Settings.RequestUrlPrefix))
                {
                    PathString prefix = "/" + shellContext.Settings.RequestUrlPrefix;
                    httpContext.Request.PathBase += prefix;
                    httpContext.Request.Path.StartsWithSegments(prefix, StringComparison.OrdinalIgnoreCase, out PathString remainingPath);
                    httpContext.Request.Path = remainingPath;
                }

                await _next(httpContext);
            }
        }
    }
}
