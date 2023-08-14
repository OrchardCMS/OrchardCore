using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Modules;

public static class ShellPipelineExtensions
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores = new();

    /// <summary>
    /// Builds the tenant pipeline atomically.
    /// </summary>
    public static async Task BuildPipelineAsync(this ShellContext context)
    {
        var semaphore = _semaphores.GetOrAdd(context.Settings.Name, _ => new SemaphoreSlim(1));

        await semaphore.WaitAsync();
        try
        {
            if (!context.HasPipeline())
            {
                context.Pipeline = context.BuildPipeline();
            }
        }
        finally
        {
            semaphore.Release();
        }
    }

    /// <summary>
    /// Builds the tenant pipeline.
    /// </summary>
    internal static IShellPipeline BuildPipeline(this ShellContext context)
    {
        var features = context.ServiceProvider.GetService<IServer>()?.Features;
        var appBuilder = new ApplicationBuilder(context.ServiceProvider, features ?? new FeatureCollection());
        var startupFilters = appBuilder.ApplicationServices.GetService<IEnumerable<IStartupFilter>>();
        var shellPipeline = new ShellRequestPipeline();

        Action<IApplicationBuilder> configure = builder =>
        {
            builder.ConfigureShellPipeline();
        };

        foreach (var filter in startupFilters.Reverse())
        {
            configure = filter.Configure(configure);
        }

        configure(appBuilder);

        shellPipeline.Next = appBuilder.Build();

        return shellPipeline;
    }

    /// <summary>
    /// Configures the tenant pipeline.
    /// </summary>
    internal static void ConfigureShellPipeline(this IApplicationBuilder builder)
    {
        var startups = appBuilder.ApplicationServices.GetServices<IStartup>();

        // 'IStartup' instances are ordered by module dependencies with a 'ConfigureOrder' of 0 by default.
        // 'OrderBy' performs a stable sort, so the order is preserved among equal 'ConfigureOrder' values.

        startups = startups.OrderBy(s => s.ConfigureOrder);
        appBuilder.UseRouting().UseEndpoints(routes =>
        {
            foreach (var startup in startups)
            {
                startup.Configure(builder, routes, ShellScope.Services);
            }
        });
    }
}
