namespace OrchardCore.Settings;

/// <summary>
/// Defines the strategy for merging settings from database and configuration file sources.
/// </summary>
/// <typeparam name="TSettings">The type of settings to merge.</typeparam>
public interface ISettingsMergeStrategy<TSettings>
    where TSettings : class, IConfigurableSettings, new()
{
    /// <summary>
    /// Merges settings from database and configuration file sources.
    /// </summary>
    /// <param name="databaseSettings">The settings loaded from the database (Admin UI).</param>
    /// <param name="fileSettings">The settings loaded from configuration files.</param>
    /// <param name="serviceProvider">The service provider for resolving dependencies.</param>
    /// <returns>The merged settings.</returns>
    TSettings Merge(TSettings databaseSettings, TSettings fileSettings, IServiceProvider serviceProvider);

    /// <summary>
    /// Determines the configuration source for a specific property.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <param name="databaseValue">The value from the database.</param>
    /// <param name="fileValue">The value from the configuration file.</param>
    /// <param name="uiDisabled">Whether UI configuration is globally disabled.</param>
    /// <returns>The effective configuration source.</returns>
    ConfigurationSource DeterminePropertySource(string propertyName, object databaseValue, object fileValue, bool uiDisabled);
}
