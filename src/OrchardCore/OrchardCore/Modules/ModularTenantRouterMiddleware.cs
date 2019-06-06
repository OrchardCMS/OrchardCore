using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Modules
{
    /// <summary>
    /// Handles a request by forwarding it to the tenant specific <see cref="IRouter"/> instance.
    /// It also initializes the middlewares for the requested tenant on the first request.
    /// </summary>
    public class ModularTenantRouterMiddleware
    {
        private readonly IFeatureCollection _features;
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores = new ConcurrentDictionary<string, SemaphoreSlim>();

        public ModularTenantRouterMiddleware(
            IFeatureCollection features,
            RequestDelegate next,
            ILogger<ModularTenantRouterMiddleware> logger)
        {
            _features = features;
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Begin Routing Request");
            }

            var shellContext = ShellScope.Context;

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

            // Do we need to rebuild the pipeline ?
            if (shellContext.Pipeline == null)
            {
                var semaphore = _semaphores.GetOrAdd(shellContext.Settings.Name, (name) => new SemaphoreSlim(1));

                // Building a pipeline for a given shell can't be done by two requests.
                await semaphore.WaitAsync();

                try
                {
                    if (shellContext.Pipeline == null)
                    {
                        shellContext.Pipeline = BuildTenantPipeline();
                    }
                }

                finally
                {
                    semaphore.Release();
                    _semaphores.TryRemove(shellContext.Settings.Name, out semaphore);
                }
            }

            await shellContext.Pipeline.Invoke(httpContext);
        }

        // Build the middleware pipeline for the current tenant
        private IShellPipeline BuildTenantPipeline()
        {
            var appBuilder = new ApplicationBuilder(ShellScope.Context.ServiceProvider, _features);

            // Create a nested pipeline to configure the tenant middleware pipeline
            var startupFilters = appBuilder.ApplicationServices.GetService<IEnumerable<IStartupFilter>>();

            var shellPipeline = new ShellRequestPipeline();

            Action<IApplicationBuilder> configure = builder =>
            {
                shellPipeline.Router = ConfigureTenantPipeline(builder);
            };

            foreach (var filter in startupFilters.Reverse())
            {
                configure = filter.Configure(configure);
            }

            configure(appBuilder);

            shellPipeline.Next = appBuilder.Build();

            return shellPipeline;
        }

        private IRouter ConfigureTenantPipeline(IApplicationBuilder appBuilder)
        {
            var startups = appBuilder.ApplicationServices.GetServices<IStartup>();

            // IStartup instances are ordered by module dependency with an Order of 0 by default.
            // OrderBy performs a stable sort so order is preserved among equal Order values.
            startups = startups.OrderBy(s => s.Order);

            var tenantRouteBuilder = appBuilder.ApplicationServices.GetService<IModularTenantRouteBuilder>();
            var routeBuilder = tenantRouteBuilder.Build(appBuilder);

            // In the case of several tenants, they will all be checked by ShellSettings. To optimize
            // the TenantRoute resolution we can create a single Router type that would index the
            // TenantRoute object by their ShellSettings. This way there would just be one lookup.
            // And the ShellSettings test in TenantRoute would also be useless.
            foreach (var startup in startups)
            {
                startup.Configure(appBuilder, routeBuilder, ShellScope.Services);
            }

            tenantRouteBuilder.Configure(routeBuilder);

            var router = routeBuilder.Build();

            appBuilder.UseRouter(router);

            return router;
        }
    }
}
