namespace OrchardCore.Settings;

/// <summary>
/// Service for retrieving settings with proper merging of database and configuration file sources.
/// </summary>
/// <typeparam name="TSettings">The type of settings to retrieve.</typeparam>
public interface IConfigurableSettingsService<TSettings>
    where TSettings : class, IConfigurableSettings, new()
{
    /// <summary>
    /// Gets the effective settings after merging database and configuration file sources.
    /// </summary>
    /// <returns>The merged settings.</returns>
    Task<TSettings> GetEffectiveSettingsAsync();

    /// <summary>
    /// Gets the metadata describing the configuration state of all properties.
    /// </summary>
    /// <returns>The settings configuration metadata.</returns>
    Task<SettingsConfigurationMetadata> GetMetadataAsync();

    /// <summary>
    /// Gets the settings as stored in the database (from Admin UI).
    /// </summary>
    /// <returns>The database settings.</returns>
    Task<TSettings> GetDatabaseSettingsAsync();

    /// <summary>
    /// Gets the settings as configured in configuration files.
    /// </summary>
    /// <returns>The file configuration settings.</returns>
    TSettings GetFileConfigurationSettings();

    /// <summary>
    /// Gets the <see cref="ConfigurationPropertyAttribute"/> for a specific property.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <returns>The attribute, or <c>null</c> if not found.</returns>
    ConfigurationPropertyAttribute GetPropertyAttribute(string propertyName);

    /// <summary>
    /// Determines whether a property can be configured via configuration files.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <returns><c>true</c> if the property can be configured via files; otherwise, <c>false</c>.</returns>
    bool CanConfigureViaFile(string propertyName);

    /// <summary>
    /// Determines whether a property can be configured via the Admin UI.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <returns><c>true</c> if the property can be configured via UI; otherwise, <c>false</c>.</returns>
    bool CanConfigureViaUI(string propertyName);
}
