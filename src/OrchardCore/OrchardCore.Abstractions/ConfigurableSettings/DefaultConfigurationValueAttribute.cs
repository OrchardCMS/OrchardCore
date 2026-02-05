namespace OrchardCore.Settings;

/// <summary>
/// Provides a default value for a configuration property when both database and file sources are empty.
/// </summary>
/// <remarks>
/// Use this attribute to specify fallback values that will be used when no configuration is provided.
/// The value is used after merge strategy resolution if the result would otherwise be null/empty.
/// </remarks>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class DefaultConfigurationValueAttribute : Attribute
{
    /// <summary>
    /// Gets the default value to use when no configuration is provided.
    /// </summary>
    public object Value { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="DefaultConfigurationValueAttribute"/> with the specified value.
    /// </summary>
    /// <param name="value">The default value.</param>
    public DefaultConfigurationValueAttribute(object value)
    {
        Value = value;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="DefaultConfigurationValueAttribute"/> with a string value.
    /// </summary>
    /// <param name="value">The default string value.</param>
    public DefaultConfigurationValueAttribute(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="DefaultConfigurationValueAttribute"/> with an integer value.
    /// </summary>
    /// <param name="value">The default integer value.</param>
    public DefaultConfigurationValueAttribute(int value)
    {
        Value = value;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="DefaultConfigurationValueAttribute"/> with a boolean value.
    /// </summary>
    /// <param name="value">The default boolean value.</param>
    public DefaultConfigurationValueAttribute(bool value)
    {
        Value = value;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="DefaultConfigurationValueAttribute"/> with a double value.
    /// </summary>
    /// <param name="value">The default double value.</param>
    public DefaultConfigurationValueAttribute(double value)
    {
        Value = value;
    }
}
