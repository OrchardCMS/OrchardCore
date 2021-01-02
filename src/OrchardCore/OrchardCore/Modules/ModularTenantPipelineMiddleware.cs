using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Modules
{
    /// <summary>
    /// Handles a request by forwarding it to the tenant specific pipeline.
    /// It also builds the pipeline of a given tenant on the first request.
    /// </summary>
    public class ModularTenantPipelineMiddleware
    {
        private readonly IFeatureCollection _features;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores = new ConcurrentDictionary<string, SemaphoreSlim>();

        public ModularTenantPipelineMiddleware(
            IFeatureCollection features,
            RequestDelegate _,
            ILogger<ModularTenantPipelineMiddleware> logger)
        {
            _features = features;
            _logger = logger;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            // Do we need to rebuild the pipeline?
            var shellContext = ShellScope.Context;
            if (shellContext.Pipeline == null)
            {
                await InitializePipelineAsync(shellContext);
            }

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Execute the '{TenantName}' tenant pipeline.", shellContext.Settings.Name);
            }

            await shellContext.Pipeline.Invoke(httpContext);
        }

        private async Task InitializePipelineAsync(ShellContext shellContext)
        {
            var semaphore = _semaphores.GetOrAdd(shellContext.Settings.Name, _ => new SemaphoreSlim(1));

            // Building a pipeline for a given shell can't be done by two requests.
            await semaphore.WaitAsync();

            try
            {
                if (shellContext.Pipeline == null)
                {
                    if (_logger.IsEnabled(LogLevel.Information))
                    {
                        _logger.LogInformation("Build the '{TenantName}' tenant pipeline.", shellContext.Settings.Name);
                    }

                    shellContext.Pipeline = BuildTenantPipeline();
                }
            }
            finally
            {
                semaphore.Release();
            }
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
                ConfigureTenantPipeline(builder);
            };

            foreach (var filter in startupFilters.Reverse())
            {
                configure = filter.Configure(configure);
            }

            configure(appBuilder);

            shellPipeline.Next = appBuilder.Build();

            return shellPipeline;
        }

        private void ConfigureTenantPipeline(IApplicationBuilder appBuilder)
        {
            var startups = appBuilder.ApplicationServices.GetServices<IStartup>();

            // IStartup instances are ordered by module dependency with an 'ConfigureOrder' of 0 by default.
            // OrderBy performs a stable sort so order is preserved among equal 'ConfigureOrder' values.
            startups = startups.OrderBy(s => s.ConfigureOrder);

            appBuilder.UseRouting().UseEndpoints(routes =>
            {
                foreach (var startup in startups)
                {
                    startup.Configure(appBuilder, routes, ShellScope.Services);
                }
            });
        }
    }
}
