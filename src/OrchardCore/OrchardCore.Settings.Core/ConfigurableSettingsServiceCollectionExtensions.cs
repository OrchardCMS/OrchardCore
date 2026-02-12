using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Settings.Services;

namespace OrchardCore.Settings;

/// <summary>
/// Extension methods for registering configurable settings services.
/// </summary>
public static class ConfigurableSettingsServiceCollectionExtensions
{
    /// <summary>
    /// Adds the configurable settings service for the specified settings type.
    /// </summary>
    /// <typeparam name="TSettings">The type of settings to configure.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configurationKey">The configuration key for binding file settings (e.g., "OrchardCore_MyModule").</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddConfigurableSettings<TSettings>(
        this IServiceCollection services,
        string configurationKey)
        where TSettings : class, IConfigurableSettings, new()
    {
        return services.AddConfigurableSettings<TSettings, AttributeBasedSettingsMergeStrategy<TSettings>>(configurationKey);
    }

    /// <summary>
    /// Adds the configurable settings service for the specified settings type with a custom merge strategy.
    /// </summary>
    /// <typeparam name="TSettings">The type of settings to configure.</typeparam>
    /// <typeparam name="TMergeStrategy">The type of merge strategy to use.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configurationKey">The configuration key for binding file settings.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddConfigurableSettings<TSettings, TMergeStrategy>(
        this IServiceCollection services,
        string configurationKey)
        where TSettings : class, IConfigurableSettings, new()
        where TMergeStrategy : class, ISettingsMergeStrategy<TSettings>
    {
        // Register the merge strategy as singleton (it's stateless)
        services.TryAddSingleton<ISettingsMergeStrategy<TSettings>, TMergeStrategy>();

        // Register the service as scoped (for UI/metadata operations)
        services.TryAddScoped<IConfigurableSettingsService<TSettings>>(sp =>
        {
            var siteService = sp.GetRequiredService<ISiteService>();
            var shellConfiguration = sp.GetRequiredService<IShellConfiguration>();
            var mergeStrategy = sp.GetRequiredService<ISettingsMergeStrategy<TSettings>>();

            return new ConfigurableSettingsService<TSettings>(
                siteService,
                shellConfiguration,
                mergeStrategy,
                sp,
                configurationKey);
        });

        // Also register a factory for creating instances that can be used from singleton contexts
        services.TryAddSingleton<ConfigurableSettingsServiceFactory<TSettings>>(sp =>
        {
            return new ConfigurableSettingsServiceFactory<TSettings>(
                sp.GetRequiredService<IShellConfiguration>(),
                sp.GetRequiredService<ISettingsMergeStrategy<TSettings>>(),
                configurationKey);
        });

        return services;
    }

    /// <summary>
    /// Adds a custom property merge function for use with <see cref="PropertyMergeStrategy.Custom"/>.
    /// </summary>
    /// <typeparam name="TMergeFunction">The type of merge function to register.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddPropertyMergeFunction<TMergeFunction>(this IServiceCollection services)
        where TMergeFunction : class, IPropertyMergeFunction
    {
        services.TryAddSingleton<TMergeFunction>();
        services.TryAddSingleton<IPropertyMergeFunction, TMergeFunction>();
        return services;
    }
}
