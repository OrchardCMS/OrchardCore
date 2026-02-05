namespace OrchardCore.Settings;

/// <summary>
/// Provides context for custom property merge operations.
/// </summary>
public class PropertyMergeContext
{
    /// <summary>
    /// Gets the name of the property being merged.
    /// </summary>
    public string PropertyName { get; init; }

    /// <summary>
    /// Gets the type of the property.
    /// </summary>
    public Type PropertyType { get; init; }

    /// <summary>
    /// Gets the type of the settings class.
    /// </summary>
    public Type SettingsType { get; init; }

    /// <summary>
    /// Gets the <see cref="ConfigurationPropertyAttribute"/> applied to the property, if any.
    /// </summary>
    public ConfigurationPropertyAttribute Attribute { get; init; }

    /// <summary>
    /// Gets or sets whether UI configuration is globally disabled for these settings.
    /// </summary>
    public bool DisableUIConfiguration { get; init; }

    /// <summary>
    /// Gets the service provider for resolving dependencies during merge operations.
    /// </summary>
    public IServiceProvider ServiceProvider { get; init; }
}

/// <summary>
/// Defines custom merge logic for a configuration property.
/// </summary>
/// <remarks>
/// Implement this interface to create custom merge strategies for specific properties.
/// Register the implementation and reference it via <see cref="ConfigurationPropertyAttribute.CustomMergeFunction"/>.
/// </remarks>
public interface IPropertyMergeFunction
{
    /// <summary>
    /// Merges the database and file configuration values according to custom logic.
    /// </summary>
    /// <param name="databaseValue">The value from the database (Admin UI).</param>
    /// <param name="fileValue">The value from the configuration file.</param>
    /// <param name="context">Context information about the merge operation.</param>
    /// <returns>The merged value.</returns>
    object Merge(object databaseValue, object fileValue, PropertyMergeContext context);
}
