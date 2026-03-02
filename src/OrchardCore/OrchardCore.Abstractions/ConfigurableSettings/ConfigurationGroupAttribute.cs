namespace OrchardCore.Settings;

/// <summary>
/// Groups related configuration properties together in the Admin UI.
/// </summary>
/// <remarks>
/// Properties with the same <see cref="GroupName"/> will be displayed together
/// in the Admin UI, improving organization of complex settings.
/// </remarks>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class ConfigurationGroupAttribute : Attribute
{
    /// <summary>
    /// Gets the name of the group this property belongs to.
    /// </summary>
    public string GroupName { get; }

    /// <summary>
    /// Gets or sets the display order within the group.
    /// Lower values appear first. Defaults to 0.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Gets or sets the display name for the group.
    /// When not set, <see cref="GroupName"/> is used.
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// Gets or sets a description for the group.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Initializes a new instance of <see cref="ConfigurationGroupAttribute"/> with the specified group name.
    /// </summary>
    /// <param name="groupName">The name of the group.</param>
    public ConfigurationGroupAttribute(string groupName)
    {
        ArgumentException.ThrowIfNullOrEmpty(groupName);
        GroupName = groupName;
    }
}
