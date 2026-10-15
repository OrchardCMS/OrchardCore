namespace OrchardCore.Localization;

/// <summary>
/// Applies versioned translations after the standard translation providers have loaded.
/// </summary>
public interface ITranslationOverrideProvider
{
    /// <summary>
    /// Returns a dictionary containing overrides, without modifying the shared fallback dictionary.
    /// Implementations must invalidate their cached result when their source changes.
    /// Mark untrusted translation keys in <see cref="CultureDictionary.PlainTextTranslations"/> for HTML encoding.
    /// </summary>
    /// <param name="dictionary">The dictionary provided by the preceding providers.</param>
    CultureDictionary ApplyTranslations(CultureDictionary dictionary);
}
