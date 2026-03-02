namespace OrchardCore.Settings;

/// <summary>
/// Controls the merge behavior for a settings property between database (UI) and configuration file sources.
/// </summary>
/// <remarks>
/// Apply this attribute to properties in classes implementing <see cref="IConfigurableSettings"/>
/// to control how values from different configuration sources are merged.
/// </remarks>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class ConfigurationPropertyAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the merge strategy for this property.
    /// Defaults to <see cref="PropertyMergeStrategy.FileOverridesDatabase"/>.
    /// </summary>
    public PropertyMergeStrategy MergeStrategy { get; set; } = PropertyMergeStrategy.FileOverridesDatabase;

    /// <summary>
    /// Gets or sets whether this property can be configured via configuration files (appsettings.json).
    /// Defaults to <c>true</c>.
    /// </summary>
    /// <remarks>
    /// When <c>false</c>, the property will only use database values, regardless of file configuration.
    /// </remarks>
    public bool AllowFileConfiguration { get; set; } = true;

    /// <summary>
    /// Gets or sets whether this property can be configured via the Admin UI.
    /// Defaults to <c>true</c>.
    /// </summary>
    /// <remarks>
    /// When <c>false</c>, the property will be displayed as read-only in the Admin UI.
    /// This is useful for secrets or production-only settings.
    /// </remarks>
    public bool AllowUIConfiguration { get; set; } = true;

    /// <summary>
    /// Gets or sets a friendly display name for this property in the Admin UI.
    /// When not set, the property name will be used.
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the type of a custom merge function to use when <see cref="MergeStrategy"/>
    /// is set to <see cref="PropertyMergeStrategy.Custom"/>.
    /// </summary>
    /// <remarks>
    /// The type must implement <see cref="IPropertyMergeFunction"/>.
    /// </remarks>
    public Type CustomMergeFunction { get; set; }

    /// <summary>
    /// Gets or sets the priority for conflict resolution.
    /// Higher values indicate higher priority. Defaults to 0.
    /// </summary>
    /// <remarks>
    /// Used when multiple configuration sources have values and the merge strategy
    /// needs additional hints for resolution.
    /// </remarks>
    public int Priority { get; set; }

    /// <summary>
    /// Gets or sets a description for this property that will be shown in the Admin UI.
    /// </summary>
    public string Description { get; set; }
}
