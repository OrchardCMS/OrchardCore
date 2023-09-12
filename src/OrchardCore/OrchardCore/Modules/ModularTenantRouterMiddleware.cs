using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Modules
{
    /// <summary>
    /// Handles a request by forwarding it to the tenant specific pipeline.
    /// It also initializes the middlewares for the requested tenant on the first request.
    /// </summary>
    public class ModularTenantRouterMiddleware
    {
        private readonly ILogger _logger;

        public ModularTenantRouterMiddleware(RequestDelegate _, ILogger<ModularTenantRouterMiddleware> logger)
            => _logger = logger;

        public async Task Invoke(HttpContext httpContext)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Begin Routing Request");
            }

            var shellContext = ShellScope.Context;

            // Define a new 'PathBase' for the current request based on the tenant 'RequestUrlPrefix'.
            // Because IIS or another middleware might have already set it, we just append the prefix.
            // This allows to use any helper accepting the '~/' path to resolve the tenant's base url.
            if (!String.IsNullOrEmpty(shellContext.Settings.RequestUrlPrefix))
            {
                PathString prefix = "/" + shellContext.Settings.RequestUrlPrefix;
                httpContext.Request.PathBase += prefix;
                httpContext.Request.Path.StartsWithSegments(prefix, StringComparison.OrdinalIgnoreCase, out var remainingPath);
                httpContext.Request.Path = remainingPath;
            }

            // Do we need to rebuild the pipeline?
            if (!shellContext.HasPipeline())
            {
                await shellContext.BuildPipelineAsync();
            }

            await shellContext.Pipeline.Invoke(httpContext);
        }
    }
}
