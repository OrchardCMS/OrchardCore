using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;

namespace OrchardCore.Modules
{
    /// <summary>
    /// This middleware selects a tenant given the current request.
    /// </summary>
    public class ModularTenantMatcherMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IShellHost _shellHost;
        private readonly IRunningShellTable _runningShellTable;

        public ModularTenantMatcherMiddleware(
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

                // Capture the shell settings and original path infos.
                httpContext.Features.Set(new ShellSettingsFeature
                {
                    ShellSettings = shellSettings,
                    OriginalPath = httpContext.Request.Path,
                    OriginalPathBase = httpContext.Request.PathBase
                });

                // Define a PathBase for the current request that is the RequestUrlPrefix.
                // This will allow any view to reference ~/ as the tenant's base url.
                // Because IIS or another middleware might have already set it, we just append the tenant prefix value.
                if (!String.IsNullOrEmpty(shellSettings.RequestUrlPrefix))
                {
                    PathString prefix = "/" + shellSettings.RequestUrlPrefix;
                    httpContext.Request.PathBase += prefix;
                    httpContext.Request.Path.StartsWithSegments(prefix, StringComparison.OrdinalIgnoreCase, out PathString remainingPath);
                    httpContext.Request.Path = remainingPath;
                }

                await _next(httpContext);
            }
        }
    }
}
