using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Builders;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for configuring services with async initialization during tenant container creation.
/// </summary>
/// <remarks>
/// <para>
/// These methods enable asynchronous configuration of options that runs during tenant container creation,
/// before the first request is processed. This is useful for options that need to:
/// </para>
/// <list type="bullet">
///   <item><description>Query the database or external services</description></item>
///   <item><description>Perform async I/O during configuration</description></item>
///   <item><description>Load settings that require async operations</description></item>
/// </list>
/// <para>
/// Options configured with <see cref="ConfigureAsync{TOptions}(IServiceCollection, Func{IServiceProvider, TOptions, ValueTask})"/>
/// are registered using the standard ASP.NET Core options pattern and can be resolved via <see cref="IOptions{TOptions}"/>.
/// Both sync configuration (via <c>services.Configure&lt;TOptions&gt;</c>) and async configuration can be combined
/// on the same options type.
/// </para>
/// <para>
/// <b>Important:</b> Async configuration only works with <see cref="IOptions{TOptions}"/>. When using
/// <see cref="IOptionsSnapshot{TOptions}"/> or <see cref="IOptionsMonitor{TOptions}"/>, async configurations
/// will be ignored because these interfaces create new options instances per scope or on configuration changes,
/// bypassing the async-configured singleton instance.
/// </para>
/// <para>
/// <b>Configuration order:</b> The execution order for options configuration is:
/// </para>
/// <list type="number">
///   <item><description>All <see cref="IConfigureOptions{TOptions}"/> (sync, in registration order)</description></item>
///   <item><description>All <see cref="IPostConfigureOptions{TOptions}"/> (sync, in registration order)</description></item>
///   <item><description>All <see cref="IAsyncConfigureOptions{TOptions}"/> via <c>ConfigureAsync</c> (async, in registration order)</description></item>
///   <item><description>All <see cref="IPostAsyncConfigureOptions{TOptions}"/> via <c>PostConfigureAsync</c> (async, in registration order)</description></item>
/// </list>
/// <para>
/// Async configuration cannot be interleaved with sync configuration - async always runs after all sync configs.
/// </para>
/// <para>
/// <b>Execution timing:</b> Async configuration runs during tenant container creation in
/// <c>ShellContextFactory.CreateDescribedContextAsync</c>, which occurs on the first request to a tenant.
/// This is different from <c>IModularTenantEvents.ActivatedAsync</c> which runs per-request scope.
/// </para>
/// </remarks>
/// <example>
/// <para><b>Using a delegate:</b></para>
/// <code>
/// services.ConfigureAsync&lt;MyOptions&gt;(async (sp, options) =>
/// {
///     var siteService = sp.GetRequiredService&lt;ISiteService&gt;();
///     var settings = await siteService.GetSettingsAsync&lt;MySettings&gt;();
///     options.SomeValue = settings.Value;
/// });
/// </code>
/// <para><b>Using IAsyncConfigureOptions:</b></para>
/// <code>
/// public class MyOptionsConfiguration : IAsyncConfigureOptions&lt;MyOptions&gt;
/// {
///     private readonly ISiteService _siteService;
///     
///     public MyOptionsConfiguration(ISiteService siteService)
///         => _siteService = siteService;
///     
///     public async ValueTask ConfigureAsync(MyOptions options)
///     {
///         var settings = await _siteService.GetSettingsAsync&lt;MySettings&gt;();
///         options.SomeValue = settings.Value;
///     }
/// }
/// 
/// // In Startup:
/// services.ConfigureAsync&lt;MyOptions, MyOptionsConfiguration&gt;();
/// </code>
/// </example>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers a delegate to be invoked asynchronously just after a tenant container is created.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="initializeAsync">The async delegate to invoke during tenant initialization.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// The delegate runs once per tenant container creation, before the first request scope is created.
    /// Use this for low-level async initialization that doesn't fit the options pattern.
    /// </remarks>
    public static IServiceCollection Initialize(this IServiceCollection services, Func<IServiceProvider, ValueTask> initializeAsync)
        => services.Configure<ShellContainerOptions>(options => options.Initializers.Add(initializeAsync));

    /// <summary>
    /// Registers a delegate used to configure asynchronously a type of options just after a tenant container is created.
    /// </summary>
    /// <typeparam name="TOptions">The options type to configure.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configureAsync">The async delegate that configures the options instance.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// <para>
    /// The options are registered using the standard ASP.NET Core options pattern and can be
    /// resolved via <see cref="IOptions{TOptions}"/>. The async configuration delegate runs
    /// during tenant container creation, before the first request is processed.
    /// </para>
    /// <para>
    /// This can be combined with sync configuration via <c>services.Configure&lt;TOptions&gt;</c>.
    /// All sync configurations run first (in registration order), then all async configurations
    /// run (in registration order). Async configuration cannot be interleaved with sync configuration.
    /// </para>
    /// </remarks>
    public static IServiceCollection ConfigureAsync<TOptions>(
        this IServiceCollection services, Func<IServiceProvider, TOptions, ValueTask> configureAsync)
        where TOptions : class, new()
    {
        services.Configure<TOptions>(_ => { });

        services.Initialize(sp =>
        {
            var options = sp.GetRequiredService<IOptions<TOptions>>().Value;

            return configureAsync(sp, options);
        });

        return services;
    }

    /// <summary>
    /// Registers an <see cref="IAsyncConfigureOptions{TOptions}"/> implementation used to configure
    /// asynchronously a type of options just after a tenant container is created.
    /// </summary>
    /// <typeparam name="TOptions">The options type to configure.</typeparam>
    /// <typeparam name="TConfigure">The <see cref="IAsyncConfigureOptions{TOptions}"/> implementation type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// <para>
    /// The <typeparamref name="TConfigure"/> implementation is registered as a transient service
    /// and its <see cref="IAsyncConfigureOptions{TOptions}.ConfigureAsync"/> method is invoked
    /// during tenant container creation.
    /// </para>
    /// <para>
    /// Use this overload when the configuration logic requires dependency injection or is complex
    /// enough to warrant a dedicated class.
    /// </para>
    /// </remarks>
    public static IServiceCollection ConfigureAsync<TOptions, TConfigure>(this IServiceCollection services)
        where TOptions : class, new()
        where TConfigure : IAsyncConfigureOptions<TOptions>
    {
        services.Configure<TOptions>(_ => { });

        if (!services.Any(d => d.ServiceType == typeof(TConfigure)))
        {
            services.AddTransient(typeof(TConfigure));
            services.Initialize(sp =>
            {
                var options = sp.GetRequiredService<IOptions<TOptions>>().Value;
                var setup = sp.GetRequiredService<TConfigure>();
                var logger = sp.GetRequiredService<ILogger<TConfigure>>();

                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug("Invoking the ConfigureAsync method on '{ConfigureType}' to configure the '{OptionsType}'", typeof(TConfigure).FullName, typeof(TOptions).FullName);
                }

                return setup.ConfigureAsync(options);
            });
        }

        return services;
    }

    /// <summary>
    /// Registers a delegate used to post-configure asynchronously a type of options after all
    /// <see cref="IAsyncConfigureOptions{TOptions}"/> have run.
    /// </summary>
    /// <typeparam name="TOptions">The options type to post-configure.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="postConfigureAsync">The async delegate that post-configures the options instance.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// Post-configuration runs after all async configuration, making it suitable for validation
    /// or finalization that depends on the fully configured options state.
    /// </remarks>
    public static IServiceCollection PostConfigureAsync<TOptions>(
        this IServiceCollection services, Func<IServiceProvider, TOptions, ValueTask> postConfigureAsync)
        where TOptions : class, new()
    {
        services.Configure<TOptions>(_ => { });

        services.Initialize(sp =>
        {
            var options = sp.GetRequiredService<IOptions<TOptions>>().Value;

            return postConfigureAsync(sp, options);
        });

        return services;
    }

    /// <summary>
    /// Registers an <see cref="IPostAsyncConfigureOptions{TOptions}"/> implementation used to post-configure
    /// asynchronously a type of options after all <see cref="IAsyncConfigureOptions{TOptions}"/> have run.
    /// </summary>
    /// <typeparam name="TOptions">The options type to post-configure.</typeparam>
    /// <typeparam name="TPostConfigure">The <see cref="IPostAsyncConfigureOptions{TOptions}"/> implementation type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// <para>
    /// The <typeparamref name="TPostConfigure"/> implementation is registered as a transient service
    /// and its <see cref="IPostAsyncConfigureOptions{TOptions}.PostConfigureAsync"/> method is invoked
    /// during tenant container creation, after all <see cref="IAsyncConfigureOptions{TOptions}"/> have run.
    /// </para>
    /// <para>
    /// Use post-configuration for validation, finalization, or any logic that depends on the fully
    /// configured options state.
    /// </para>
    /// </remarks>
    public static IServiceCollection PostConfigureAsync<TOptions, TPostConfigure>(this IServiceCollection services)
        where TOptions : class, new()
        where TPostConfigure : IPostAsyncConfigureOptions<TOptions>
    {
        services.Configure<TOptions>(_ => { });

        if (!services.Any(d => d.ServiceType == typeof(TPostConfigure)))
        {
            services.AddTransient(typeof(TPostConfigure));
            services.Initialize(sp =>
            {
                var options = sp.GetRequiredService<IOptions<TOptions>>().Value;
                var setup = sp.GetRequiredService<TPostConfigure>();
                var logger = sp.GetRequiredService<ILogger<TPostConfigure>>();

                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug("Invoking the PostConfigureAsync method on '{PostConfigureType}' to post-configure the '{OptionsType}'", typeof(TPostConfigure).FullName, typeof(TOptions).FullName);
                }

                return setup.PostConfigureAsync(options);
            });
        }

        return services;
    }
}
