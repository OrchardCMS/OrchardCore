namespace OrchardCore.Localization;

/// <summary>
/// Represents an immutable untranslated value and the type that supplies its localization context.
/// </summary>
/// <remarks>
/// This type does not translate, format, or encode the value. Choose a string or HTML localizer
/// when the value is displayed. Sources can be shared across instances, tenants, and cultures.
/// Use <see cref="Create(string, Type)"/> or <see cref="Create{T}(string)"/> to declare sources
/// with explicit calls that source-extraction tools can discover.
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
    /// Initializes a new instance of the <see cref="LocalizationSource"/> class with a note for translators.
    /// </summary>
    /// <param name="value">The untranslated value, used as the localization key.</param>
    /// <param name="type">The source type of the localizer, or <see langword="null"/> for a value without a localization context.</param>
    /// <param name="note">
    /// A note explaining the meaning or usage of the value to translators. It is only written to translation
    /// templates by the extraction, and is neither stored nor used at runtime.
    /// </param>
    public LocalizationSource(string value, Type type, string note)
        : this(value, type)
    {
        _ = note;
    }

    /// <summary>
    /// Creates an immutable untranslated source with an optional localization context.
    /// </summary>
    /// <param name="value">The untranslated value, used as the localization key.</param>
    /// <param name="type">The source type of the localizer, or <see langword="null"/> for a value without a localization context.</param>
    /// <returns>A new untranslated source with the specified value and source type.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    [SkipLocalizationExtraction]
    public static LocalizationSource Create(string value, Type type = null)
    {
        return new LocalizationSource(value, type);
    }

    /// <summary>
    /// Creates an immutable untranslated source with an optional localization context and a note for translators.
    /// </summary>
    /// <param name="value">The untranslated value, used as the localization key.</param>
    /// <param name="type">The source type of the localizer, or <see langword="null"/> for a value without a localization context.</param>
    /// <param name="note">
    /// A note explaining the meaning or usage of the value to translators. It is only written to translation
    /// templates by the extraction, and is neither stored nor used at runtime.
    /// </param>
    /// <returns>A new untranslated source with the specified value and source type.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    [SkipLocalizationExtraction]
    public static LocalizationSource Create(string value, Type type, string note)
    {
        return new LocalizationSource(value, type, note);
    }

    /// <summary>
    /// Creates an immutable untranslated source using <typeparamref name="T"/> as its localization context.
    /// </summary>
    /// <typeparam name="T">The source type of the localizer.</typeparam>
    /// <param name="value">The untranslated value, used as the localization key.</param>
    /// <returns>A new untranslated source with the specified value and source type.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    [SkipLocalizationExtraction]
    public static LocalizationSource Create<T>(string value)
    {
        return Create(value, typeof(T));
    }

    /// <summary>
    /// Creates an immutable untranslated source using <typeparamref name="T"/> as its localization context, with a
    /// note for translators.
    /// </summary>
    /// <typeparam name="T">The source type of the localizer.</typeparam>
    /// <param name="value">The untranslated value, used as the localization key.</param>
    /// <param name="note">
    /// A note explaining the meaning or usage of the value to translators. It is only written to translation
    /// templates by the extraction, and is neither stored nor used at runtime.
    /// </param>
    /// <returns>A new untranslated source with the specified value and source type.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    [SkipLocalizationExtraction]
    public static LocalizationSource Create<T>(string value, string note)
    {
        return Create(value, typeof(T), note);
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
