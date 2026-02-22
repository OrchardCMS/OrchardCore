using System.Collections;

namespace OrchardCore.Settings;

/// <summary>
/// Provides metadata about a configuration property's current state.
/// </summary>
public class PropertyConfigurationMetadata
{
    /// <summary>
    /// Gets or sets the property name.
    /// </summary>
    public string PropertyName { get; set; }

    /// <summary>
    /// Gets or sets the display name for the property.
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the source of the effective value.
    /// </summary>
    public ConfigurationSource Source { get; set; }

    /// <summary>
    /// Gets or sets the value stored in the database (Admin UI).
    /// </summary>
    public object DatabaseValue { get; set; }

    /// <summary>
    /// Gets or sets the value from the configuration file.
    /// </summary>
    public object FileValue { get; set; }

    /// <summary>
    /// Gets or sets the default value from <see cref="DefaultConfigurationValueAttribute"/>.
    /// </summary>
    public object DefaultValue { get; set; }

    /// <summary>
    /// Gets or sets the effective value after merge resolution.
    /// </summary>
    public object EffectiveValue { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="ConfigurationPropertyAttribute"/> applied to this property.
    /// </summary>
    public ConfigurationPropertyAttribute Attribute { get; set; }

    /// <summary>
    /// Gets or sets whether this property contains sensitive information.
    /// </summary>
    public bool IsSensitive { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="SensitiveConfigurationAttribute"/> applied to this property.
    /// </summary>
    public SensitiveConfigurationAttribute SensitiveAttribute { get; set; }

    /// <summary>
    /// Gets or sets the group name for UI organization.
    /// </summary>
    public string GroupName { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="ConfigurationGroupAttribute"/> applied to this property.
    /// </summary>
    public ConfigurationGroupAttribute GroupAttribute { get; set; }

    /// <summary>
    /// Gets or sets the property type.
    /// </summary>
    public Type PropertyType { get; set; }

    /// <summary>
    /// Gets whether this property's effective value is overridden by a configuration file.
    /// </summary>
    public bool IsOverriddenByFile => Source == ConfigurationSource.ConfigurationFile;

    /// <summary>
    /// Gets whether this property can be configured via the Admin UI.
    /// </summary>
    public bool CanConfigureViaUI => Attribute?.AllowUIConfiguration ?? true;

    /// <summary>
    /// Gets whether this property can be configured via configuration files.
    /// </summary>
    public bool CanConfigureViaFile => Attribute?.AllowFileConfiguration ?? true;

    /// <summary>
    /// Gets the merge strategy for this property.
    /// </summary>
    public PropertyMergeStrategy MergeStrategy => Attribute?.MergeStrategy ?? PropertyMergeStrategy.FileOverridesDatabase;

    /// <summary>
    /// Gets the masked value for sensitive properties.
    /// </summary>
    /// <returns>The masked string representation of the effective value, or the value as-is if not sensitive.</returns>
    public string GetMaskedValue()
    {
        if (!IsSensitive || EffectiveValue == null)
        {
            return GetDisplayValue();
        }

        var value = EffectiveValue.ToString();
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        var visibleChars = SensitiveAttribute?.VisibleCharacters ?? 4;
        var maskChar = SensitiveAttribute?.MaskCharacter ?? '•';

        if (visibleChars >= value.Length)
        {
            return new string(maskChar, value.Length);
        }

        var maskedLength = value.Length - visibleChars;
        return new string(maskChar, maskedLength) + value[maskedLength..];
    }

    /// <summary>
    /// Gets a display-friendly string representation of the effective value.
    /// </summary>
    /// <returns>A formatted string suitable for display in the UI.</returns>
    public string GetDisplayValue()
    {
        return FormatValueForDisplay(EffectiveValue);
    }

    /// <summary>
    /// Gets a display-friendly string representation of the file value.
    /// </summary>
    /// <returns>A formatted string suitable for display in the UI.</returns>
    public string GetFileDisplayValue()
    {
        return FormatValueForDisplay(FileValue);
    }

    /// <summary>
    /// Gets a display-friendly string representation of the database value.
    /// </summary>
    /// <returns>A formatted string suitable for display in the UI.</returns>
    public string GetDatabaseDisplayValue()
    {
        return FormatValueForDisplay(DatabaseValue);
    }

    private static string FormatValueForDisplay(object value)
    {
        if (value == null)
        {
            return null;
        }

        // Handle arrays and collections
        if (value is IEnumerable enumerable && value is not string)
        {
            var items = new List<string>();
            foreach (var item in enumerable)
            {
                items.Add(item?.ToString() ?? "(null)");
            }

            return items.Count == 0 ? "(empty)" : string.Join(", ", items);
        }

        return value.ToString();
    }
}
