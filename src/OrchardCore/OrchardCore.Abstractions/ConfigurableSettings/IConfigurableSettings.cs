namespace OrchardCore.Settings;

/// <summary>
/// Marker interface for settings that support configuration merging between
/// Admin UI (database) and configuration files (appsettings.json).
/// </summary>
/// <remarks>
/// Settings implementing this interface can be controlled through both the Admin UI
/// and configuration files, with declarative control over how these sources merge
/// using <see cref="ConfigurationPropertyAttribute"/>.
/// </remarks>
public interface IConfigurableSettings
{
    /// <summary>
    /// Gets or sets a value indicating whether UI configuration is disabled.
    /// When <c>true</c>, all settings will be read-only in the Admin UI and
    /// can only be configured through configuration files.
    /// </summary>
    /// <remarks>
    /// This is typically set via appsettings.json to lock down settings in production:
    /// <code>
    /// {
    ///   "OrchardCore_MyModule": {
    ///     "DisableUIConfiguration": true
    ///   }
    /// }
    /// </code>
    /// </remarks>
    bool DisableUIConfiguration { get; set; }
}
