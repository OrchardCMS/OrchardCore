namespace OrchardCore.Settings;

/// <summary>
/// Indicates the source of a configuration value.
/// </summary>
public enum ConfigurationSource
{
    /// <summary>
    /// The value is from the hardcoded default or <see cref="DefaultConfigurationValueAttribute"/>.
    /// </summary>
    Default = 0,

    /// <summary>
    /// The value is from the database (set via Admin UI).
    /// </summary>
    Database = 1,

    /// <summary>
    /// The value is from a configuration file (appsettings.json).
    /// </summary>
    ConfigurationFile = 2,
}
