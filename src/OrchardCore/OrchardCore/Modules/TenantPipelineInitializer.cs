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
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Modules;

public class TenantPipelineInitializer
{
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores = new();

    public async Task InitializeAsync(HttpContext httpContext, ShellContext shellContext, IFeatureCollection features)
    {
        // Define a new 'PathBase' for the current request based on the tenant 'RequestUrlPrefix'.
        // Because IIS or another middleware might have already set it, we just append the prefix.
        // This allows to use any helper accepting the '~/' path to resolve the tenant's base url.
        if (!String.IsNullOrEmpty(shellContext.Settings.RequestUrlPrefix))
        {
            PathString prefix = '/' + shellContext.Settings.RequestUrlPrefix;
            httpContext.Request.PathBase += prefix;
            httpContext.Request.Path.StartsWithSegments(prefix, StringComparison.OrdinalIgnoreCase, out var remainingPath);
            httpContext.Request.Path = remainingPath;
        }

        // Do we need to rebuild the pipeline?
        if (!shellContext.HasPipeline())
        {
            await InitializePipelineAsync(shellContext, features);
        }

        await shellContext.Pipeline.Invoke(httpContext);
    }

    private async Task InitializePipelineAsync(ShellContext shellContext, IFeatureCollection features)
    {
        var semaphore = _semaphores.GetOrAdd(shellContext.Settings.Name, _ => new SemaphoreSlim(1));

        // Building a pipeline for a given shell can't be done by two requests.
        await semaphore.WaitAsync();
        try
        {
            if (!shellContext.HasPipeline())
            {
                shellContext.Pipeline = BuildTenantPipeline(features);
            }
        }
        finally
        {
            semaphore.Release();
        }
    }

    // Build the middleware pipeline for the current tenant.
    private static IShellPipeline BuildTenantPipeline(IFeatureCollection features)
    {
        var appBuilder = new ApplicationBuilder(ShellScope.Context.ServiceProvider, features);

        // Create a nested pipeline to configure the tenant middleware pipeline.
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

    private static void ConfigureTenantPipeline(IApplicationBuilder appBuilder)
    {
        var startups = appBuilder.ApplicationServices.GetServices<IStartup>();

        // IStartup instances are ordered by module dependency with an 'ConfigureOrder' of 0 by default.
        // 'OrderBy' performs a stable sort so order is preserved among equal 'ConfigureOrder' values.
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
