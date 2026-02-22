namespace OrchardCore.Settings;

/// <summary>
/// Defines how a property's value should be merged between database (UI) and configuration file sources.
/// </summary>
public enum PropertyMergeStrategy
{
    /// <summary>
    /// Configuration file value overrides database value when set.
    /// This is the default behavior for most settings.
    /// </summary>
    FileOverridesDatabase = 0,

    /// <summary>
    /// Database value overrides configuration file value when set.
    /// Use when UI settings should take precedence over file defaults.
    /// </summary>
    DatabaseOverridesFile = 1,

    /// <summary>
    /// Configuration file provides default value, used only when database value is empty.
    /// Allows users to override defaults via UI.
    /// </summary>
    FileAsDefault = 2,

    /// <summary>
    /// Database provides default value, used only when file value is empty.
    /// Rare scenario where database serves as fallback.
    /// </summary>
    DatabaseAsDefault = 3,

    /// <summary>
    /// Merge both sources together.
    /// For arrays/collections, combines unique values from both sources.
    /// For dictionaries, merges key-value pairs (file takes precedence for duplicate keys).
    /// </summary>
    Merge = 4,

    /// <summary>
    /// Only database (UI) configuration is allowed.
    /// File configuration is ignored for this property.
    /// </summary>
    DatabaseOnly = 5,

    /// <summary>
    /// Only file configuration is allowed.
    /// UI will show this property as read-only.
    /// Use for secrets or production-only settings.
    /// </summary>
    FileOnly = 6,

    /// <summary>
    /// Custom merge logic is used.
    /// Requires <see cref="ConfigurationPropertyAttribute.CustomMergeFunction"/> to be set.
    /// </summary>
    Custom = 7,
}
