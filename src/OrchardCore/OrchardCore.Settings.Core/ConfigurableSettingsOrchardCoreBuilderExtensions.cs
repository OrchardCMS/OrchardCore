using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Settings;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="OrchardCoreBuilder"/> to configure settings.
/// </summary>
public static class ConfigurableSettingsOrchardCoreBuilderExtensions
{
    /// <summary>
    /// Configures settings of type <typeparamref name="TSettings"/> to be bound from configuration files.
    /// </summary>
    /// <typeparam name="TSettings">The type of settings to configure.</typeparam>
    /// <param name="builder">The Orchard Core builder.</param>
    /// <param name="configurationKey">The configuration key for binding (e.g., "OrchardCore_MyModule").</param>
    /// <returns>The builder for chaining.</returns>
    /// <remarks>
    /// This method sets up PostConfigure to bind settings from the configuration file.
    /// For full configurable settings support with merging and metadata, also use
    /// <see cref="ConfigurableSettingsServiceCollectionExtensions.AddConfigurableSettings{TSettings}"/>
    /// in the module's Startup.
    /// </remarks>
    public static OrchardCoreBuilder ConfigureSettings<TSettings>(
        this OrchardCoreBuilder builder,
        string configurationKey)
        where TSettings : class, new()
    {
        ArgumentException.ThrowIfNullOrEmpty(configurationKey);

        builder.ConfigureServices((services, serviceProvider) =>
        {
            services.PostConfigure<TSettings>(settings =>
            {
                var shellConfiguration = serviceProvider.GetService<IShellConfiguration>();
                if (shellConfiguration != null)
                {
                    var section = shellConfiguration.GetSection(configurationKey);
                    if (section.Exists())
                    {
                        section.Bind(settings);
                    }
                }
            });
        });

        return builder;
    }

    /// <summary>
    /// Configures settings of type <typeparamref name="TSettings"/> from the specified configuration section.
    /// </summary>
    /// <typeparam name="TSettings">The type of settings to configure.</typeparam>
    /// <param name="builder">The Orchard Core builder.</param>
    /// <param name="configurationKey">The configuration key for binding.</param>
    /// <param name="configureSectionName">Optional section name within the configuration key.</param>
    /// <returns>The builder for chaining.</returns>
    public static OrchardCoreBuilder ConfigureSettings<TSettings>(
        this OrchardCoreBuilder builder,
        string configurationKey,
        string configureSectionName)
        where TSettings : class, new()
    {
        ArgumentException.ThrowIfNullOrEmpty(configurationKey);

        var fullPath = string.IsNullOrEmpty(configureSectionName)
            ? configurationKey
            : $"{configurationKey}:{configureSectionName}";

        return builder.ConfigureSettings<TSettings>(fullPath);
    }
}
