using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Shell;
using Orchard.Environment.Shell.Models;

namespace Microsoft.AspNetCore.Modules
{
    /// <summary>
    /// Handles a request by forwarding it to the tenant specific <see cref="IRouter"/> instance.
    /// It also initializes the middlewares for the requested tenant on the first request.
    /// </summary>
    public class ModularTenantRouterMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly Dictionary<string, RequestDelegate> _pipelines = new Dictionary<string, RequestDelegate>();

        public ModularTenantRouterMiddleware(
            RequestDelegate next,
            ILogger<ModularTenantRouterMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Begin Routing Request");
            }


            var shellSettings = httpContext.Features.Get<ShellSettings>();

            // Define a PathBase for the current request that is the RequestUrlPrefix.
            // This will allow any view to reference ~/ as the tenant's base url.
            // Because IIS or another middleware might have already set it, we just append the tenant prefix value.
            if (!string.IsNullOrEmpty(shellSettings.RequestUrlPrefix))
            {
                httpContext.Request.PathBase += ("/" + shellSettings.RequestUrlPrefix);
                httpContext.Request.Path = httpContext.Request.Path.ToString().Substring(httpContext.Request.PathBase.Value.Length);
            }

            // TODO: Invalidate the pipeline automatically when the shell context is changed
            // such that we can reload the middlewares and the routes. Implement something similar
            // to IRunningShellTable but for the pipelines.

            // Do we need to rebuild the pipeline ?
            var rebuildPipeline = httpContext.Items["BuildPipeline"] != null;
            if (rebuildPipeline && _pipelines.ContainsKey(shellSettings.Name))
            {
                _pipelines.Remove(shellSettings.Name);
            }

            RequestDelegate pipeline;

            if (!_pipelines.TryGetValue(shellSettings.Name, out pipeline))
            {
                // Building a pipeline can't be done by two requests
                lock (_pipelines)
                {
                    if (!_pipelines.TryGetValue(shellSettings.Name, out pipeline))
                    {
                        pipeline = BuildTenantPipeline(shellSettings, httpContext.RequestServices);

                        if (shellSettings.State == TenantState.Running)
                        {
                            _pipelines.Add(shellSettings.Name, pipeline);
                        }
                    }
                }
            }

            await pipeline.Invoke(httpContext);
        }

        // Build the middleware pipeline for the current tenant
        public RequestDelegate BuildTenantPipeline(ShellSettings shellSettings, IServiceProvider serviceProvider)
        {
            var startups = serviceProvider.GetServices<IStartup>();
            var tenantRouteBuilder = serviceProvider.GetService<IModularTenantRouteBuilder>();
            
            var appBuilder = new ApplicationBuilder(serviceProvider);
            var routeBuilder = tenantRouteBuilder.Build();

            // In the case of several tenants, they will all be checked by ShellSettings. To optimize
            // the TenantRoute resolution we can create a single Router type that would index the
            // TenantRoute object by their ShellSetting. This way there would just be one lookup.
            // And the ShellSettings test in TenantRoute would also be useless.

            foreach (var startup in startups)
            {
                startup.Configure(appBuilder, routeBuilder, serviceProvider);
            }

            tenantRouteBuilder.Configure(routeBuilder);

            var router = routeBuilder.Build();

            appBuilder.UseRouter(router);

            var pipeline = appBuilder.Build();

            return pipeline;
        }
    }
}