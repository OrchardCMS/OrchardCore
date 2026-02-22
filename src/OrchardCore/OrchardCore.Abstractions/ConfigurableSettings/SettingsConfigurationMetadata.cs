using System.Collections.ObjectModel;

namespace OrchardCore.Settings;

/// <summary>
/// Provides comprehensive metadata about a settings object's configuration state.
/// </summary>
public class SettingsConfigurationMetadata
{
    private readonly Dictionary<string, PropertyConfigurationMetadata> _properties = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets or sets whether any property is configured from a file source.
    /// </summary>
    public bool IsConfiguredFromFile { get; set; }

    /// <summary>
    /// Gets or sets whether UI configuration is globally disabled for these settings.
    /// </summary>
    public bool DisableUIConfiguration { get; set; }

    /// <summary>
    /// Gets the dictionary of property metadata keyed by property name.
    /// </summary>
    public IReadOnlyDictionary<string, PropertyConfigurationMetadata> Properties
        => new ReadOnlyDictionary<string, PropertyConfigurationMetadata>(_properties);

    /// <summary>
    /// Gets or sets the type of settings this metadata describes.
    /// </summary>
    public Type SettingsType { get; set; }

    /// <summary>
    /// Gets or sets the configuration key used for file configuration.
    /// </summary>
    public string ConfigurationKey { get; set; }

    /// <summary>
    /// Adds property metadata.
    /// </summary>
    /// <param name="metadata">The property metadata to add.</param>
    public void AddProperty(PropertyConfigurationMetadata metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentException.ThrowIfNullOrEmpty(metadata.PropertyName);
        _properties[metadata.PropertyName] = metadata;
    }

    /// <summary>
    /// Gets whether a specific property is overridden by file configuration.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <returns><c>true</c> if the property is overridden by file configuration; otherwise, <c>false</c>.</returns>
    public bool IsPropertyOverridden(string propertyName)
        => _properties.TryGetValue(propertyName, out var metadata) && metadata.IsOverriddenByFile;

    /// <summary>
    /// Gets the configuration source for a specific property.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <returns>The configuration source, or <see cref="ConfigurationSource.Default"/> if not found.</returns>
    public ConfigurationSource GetPropertySource(string propertyName)
        => _properties.TryGetValue(propertyName, out var metadata)
            ? metadata.Source
            : ConfigurationSource.Default;

    /// <summary>
    /// Gets the effective value for a specific property.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <returns>The effective value, or <c>null</c> if not found.</returns>
    public object GetEffectiveValue(string propertyName)
        => _properties.TryGetValue(propertyName, out var metadata)
            ? metadata.EffectiveValue
            : null;

    /// <summary>
    /// Gets metadata for a specific property.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <returns>The property metadata, or <c>null</c> if not found.</returns>
    public PropertyConfigurationMetadata GetPropertyMetadata(string propertyName)
        => _properties.TryGetValue(propertyName, out var metadata) ? metadata : null;

    /// <summary>
    /// Gets all properties that are overridden by file configuration.
    /// </summary>
    /// <returns>A collection of property metadata for overridden properties.</returns>
    public IEnumerable<PropertyConfigurationMetadata> GetOverriddenProperties()
        => _properties.Values.Where(p => p.IsOverriddenByFile);

    /// <summary>
    /// Gets all properties grouped by their configuration group.
    /// </summary>
    /// <returns>Groups of property metadata keyed by group name (null key for ungrouped properties).</returns>
    public IEnumerable<IGrouping<string, PropertyConfigurationMetadata>> GetPropertiesByGroup()
        => _properties.Values
            .OrderBy(p => p.GroupAttribute?.Order ?? 0)
            .GroupBy(p => p.GroupName);

    /// <summary>
    /// Gets all properties that can be configured via the Admin UI.
    /// </summary>
    /// <returns>A collection of property metadata for UI-configurable properties.</returns>
    public IEnumerable<PropertyConfigurationMetadata> GetUIConfigurableProperties()
        => _properties.Values.Where(p => p.CanConfigureViaUI && !DisableUIConfiguration);

    /// <summary>
    /// Gets all sensitive properties.
    /// </summary>
    /// <returns>A collection of property metadata for sensitive properties.</returns>
    public IEnumerable<PropertyConfigurationMetadata> GetSensitiveProperties()
        => _properties.Values.Where(p => p.IsSensitive);
}
