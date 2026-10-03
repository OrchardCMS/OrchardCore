namespace OrchardCore.Localization;

/// <summary>
/// Represents an immutable untranslated value and the type that supplies its localization context.
/// </summary>
/// <remarks>
/// This type does not translate, format, or encode the value. Choose a string or HTML localizer
/// when the value is displayed. Sources can be shared across instances, tenants, and cultures.
/// </remarks>
public sealed record LocalizationSource
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizationSource"/> class.
    /// </summary>
    /// <param name="value">The untranslated value, used as the localization key.</param>
    /// <param name="type">The source type of the localizer, or <see langword="null"/> for a value without a localization context.</param>
    public LocalizationSource(string value, Type type = null)
    {
        ArgumentNullException.ThrowIfNull(value);

        Value = value;
        Type = type;
    }

    /// <summary>
    /// Gets the source type of the localizer, or <see langword="null"/> when no localization context is provided.
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// Gets the untranslated value, used as the localization key.
    /// </summary>
    public string Value { get; }
}
