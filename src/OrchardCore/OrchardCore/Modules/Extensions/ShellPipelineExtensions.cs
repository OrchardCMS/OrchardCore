using System.Collections.Concurrent;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Modules;

public static class ShellPipelineExtensions
{
    private const string EndpointRouteBuilder = "__EndpointRouteBuilder";

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
                context.Pipeline = await context.BuildPipelineInternalAsync();
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
    private static async ValueTask<IShellPipeline> BuildPipelineInternalAsync(this ShellContext context)
    {
        var features = context.ServiceProvider.GetService<IServer>()?.Features;
        var builder = new ApplicationBuilder(context.ServiceProvider, features ?? new FeatureCollection());
        var startupFilters = builder.ApplicationServices.GetService<IEnumerable<IStartupFilter>>();

        Action<IApplicationBuilder> configure = builder => { };
        foreach (var filter in startupFilters.Reverse())
        {
            configure = filter.Configure(configure);
        }

        configure(builder);

        await ConfigurePipelineAsync(builder);

        var shellPipeline = new ShellRequestPipeline
        {
            Next = builder.Build()
        };

        return shellPipeline;
    }

    /// <summary>
    /// Configures the tenant pipeline.
    /// </summary>
    private static async ValueTask ConfigurePipelineAsync(ApplicationBuilder builder)
    {
        // 'IStartup' instances are ordered by module dependencies with a 'ConfigureOrder' of 0 by default.
        // 'OrderBy' performs a stable sort, so the order is preserved among equal 'ConfigureOrder' values.
        var startups = builder.ApplicationServices.GetServices<IStartup>().OrderBy(s => s.ConfigureOrder);

        // Should be done first.
        builder.UseRouting();

        // Try to retrieve the current 'IEndpointRouteBuilder'.
        if (!builder.Properties.TryGetValue(EndpointRouteBuilder, out var obj) ||
            obj is not IEndpointRouteBuilder routes)
        {
            throw new InvalidOperationException("Failed to retrieve the current endpoint route builder.");
        }

        // Routes can be then configured outside 'UseEndpoints()'.
        var services = ShellScope.Services;
        foreach (var startup in startups)
        {
            if (startup is IAsyncStartup asyncStartup)
            {
                await asyncStartup.ConfigureAsync(builder, routes, services);
            }

            startup.Configure(builder, routes, services);
        }

        // Knowing that routes are already configured.
        builder.UseEndpoints(routes => { });
    }
}
