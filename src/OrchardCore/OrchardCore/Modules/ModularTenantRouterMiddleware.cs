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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores = new ConcurrentDictionary<string, SemaphoreSlim>();

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
                        shellContext.Pipeline = BuildTenantPipeline(shellContext.ServiceProvider, httpContext.RequestServices);
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
            // TenantRoute object by their ShellSettings. This way there would just be one lookup.
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