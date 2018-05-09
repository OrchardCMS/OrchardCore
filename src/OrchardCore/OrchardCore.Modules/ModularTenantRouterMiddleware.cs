using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Hosting.ShellBuilders;

namespace OrchardCore.Modules
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

            var shellContext = httpContext.Features.Get<ShellContext>();

            // Define a PathBase for the current request that is the RequestUrlPrefix.
            // This will allow any view to reference ~/ as the tenant's base url.
            // Because IIS or another middleware might have already set it, we just append the tenant prefix value.
            if (!string.IsNullOrEmpty(shellContext.Settings.RequestUrlPrefix))
            {
                httpContext.Request.PathBase += ("/" + shellContext.Settings.RequestUrlPrefix);
                httpContext.Request.Path = httpContext.Request.Path.ToString().Substring(httpContext.Request.PathBase.Value.Length);
            }

            // TODO: Invalidate the pipeline automatically when the shell context is changed
            // such that we can reload the middlewares and the routes. Implement something similar
            // to IRunningShellTable but for the pipelines.

            // Do we need to rebuild the pipeline ?
            var rebuildPipeline = httpContext.Items["BuildPipeline"] != null;
            if (rebuildPipeline && _pipelines.ContainsKey(shellContext.Settings.Name))
            {
                _pipelines.Remove(shellContext.Settings.Name);
            }

            RequestDelegate pipeline;

            if (!_pipelines.TryGetValue(shellContext.Settings.Name, out pipeline))
            {
                // Building a pipeline can't be done by two requests
                lock (_pipelines)
                {
                    if (!_pipelines.TryGetValue(shellContext.Settings.Name, out pipeline))
                    {
                        pipeline = BuildTenantPipeline(shellContext.ServiceProvider, httpContext.RequestServices);

                        if (shellContext.Settings.State == TenantState.Running)
                        {
                            _pipelines.Add(shellContext.Settings.Name, pipeline);
                        }
                    }
                }
            }

            await pipeline.Invoke(httpContext);
        }

        // Build the middleware pipeline for the current tenant
        public RequestDelegate BuildTenantPipeline(IServiceProvider rootServiceProvider, IServiceProvider scopeServiceProvider)
        {
            var appBuilder = new ApplicationBuilder(rootServiceProvider);

            // Create a nested pipeline to configure the tenant middleware pipeline
            var startupFilters = appBuilder.ApplicationServices.GetService<IEnumerable<IStartupFilter>>();

            Action<IApplicationBuilder> configure = builder =>
            {
                ConfigureTenantPipeline(builder, scopeServiceProvider);
            };

            foreach (var filter in startupFilters.Reverse())
            {
                configure = filter.Configure(configure);
            }

            configure(appBuilder);

            var pipeline = appBuilder.Build();

            return pipeline;
        }

        private void ConfigureTenantPipeline(IApplicationBuilder appBuilder, IServiceProvider scopeServiceProvider)
        {
            var startups = appBuilder.ApplicationServices.GetServices<IStartup>();

            // IStartup instances are ordered by module dependency with an Order of 0 by default.
            // OrderBy performs a stable sort so order is preserved among equal Order values.
            startups = startups.OrderBy(s => s.Order);

            var tenantRouteBuilder = appBuilder.ApplicationServices.GetService<IModularTenantRouteBuilder>();
            var routeBuilder = tenantRouteBuilder.Build(appBuilder);

            // In the case of several tenants, they will all be checked by ShellSettings. To optimize
            // the TenantRoute resolution we can create a single Router type that would index the
            // TenantRoute object by their ShellSetting. This way there would just be one lookup.
            // And the ShellSettings test in TenantRoute would also be useless.
            foreach (var startup in startups)
            {
                startup.Configure(appBuilder, routeBuilder, scopeServiceProvider);
            }

            tenantRouteBuilder.Configure(routeBuilder);

            var router = routeBuilder.Build();

            appBuilder.UseRouter(router);
        }
    }
}
