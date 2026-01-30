namespace OrchardCore.Settings.ViewModels;

/// <summary>
/// Base view model for configurable settings that provides access to configuration metadata.
/// </summary>
/// <typeparam name="TSettings">The type of settings this view model represents.</typeparam>
public abstract class ConfigurableSettingsViewModel<TSettings>
    where TSettings : class, IConfigurableSettings, new()
{
    /// <summary>
    /// Gets or sets the configuration metadata describing the state of all properties.
    /// </summary>
    public SettingsConfigurationMetadata Metadata { get; set; }

    /// <summary>
    /// Gets or sets whether the settings are in read-only mode due to <see cref="IConfigurableSettings.DisableUIConfiguration"/>.
    /// </summary>
    public bool IsReadOnly { get; set; }

    /// <summary>
    /// Gets whether a specific property is overridden by file configuration.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <returns><c>true</c> if the property is overridden by file configuration; otherwise, <c>false</c>.</returns>
    public bool IsPropertyOverridden(string propertyName)
        => Metadata?.IsPropertyOverridden(propertyName) ?? false;

    /// <summary>
    /// Gets the configuration source for a specific property.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <returns>The configuration source.</returns>
    public ConfigurationSource GetPropertySource(string propertyName)
        => Metadata?.GetPropertySource(propertyName) ?? ConfigurationSource.Default;

    /// <summary>
    /// Gets the effective value for a specific property.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <returns>The effective value, or <c>null</c> if not found.</returns>
    public object GetEffectiveValue(string propertyName)
        => Metadata?.GetEffectiveValue(propertyName);

    /// <summary>
    /// Gets metadata for a specific property.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <returns>The property metadata, or <c>null</c> if not found.</returns>
    public PropertyConfigurationMetadata GetPropertyMetadata(string propertyName)
        => Metadata?.GetPropertyMetadata(propertyName);

    /// <summary>
    /// Gets whether a specific property can be edited in the UI.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <returns><c>true</c> if the property can be edited; otherwise, <c>false</c>.</returns>
    public bool CanEditProperty(string propertyName)
    {
        if (IsReadOnly)
        {
            return false;
        }

        var metadata = GetPropertyMetadata(propertyName);
        return metadata?.CanConfigureViaUI ?? true;
    }

    /// <summary>
    /// Gets whether there are any file-configured overrides.
    /// </summary>
    public bool HasFileOverrides => Metadata?.IsConfiguredFromFile ?? false;

    /// <summary>
    /// Gets the display name for a specific property.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <returns>The display name.</returns>
    public string GetPropertyDisplayName(string propertyName)
    {
        var metadata = GetPropertyMetadata(propertyName);
        return metadata?.DisplayName ?? propertyName;
    }

    /// <summary>
    /// Gets the masked value for a sensitive property.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <returns>The masked value, or the actual value if not sensitive.</returns>
    public string GetMaskedValue(string propertyName)
    {
        var metadata = GetPropertyMetadata(propertyName);
        return metadata?.GetMaskedValue();
    }
}
