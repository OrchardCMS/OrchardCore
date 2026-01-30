namespace OrchardCore.DataLocalization.ViewModels;

/// <summary>
/// Represents a single translatable string.
/// </summary>
public class TranslatableStringViewModel
{
    /// <summary>
    /// The full context string (e.g., "Permissions", "Content Types", "Content Fields;TextField").
    /// </summary>
    public string Context { get; set; }

    /// <summary>
    /// The original (untranslated) string key.
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// The translated value for the current culture, or empty if not translated.
    /// </summary>
    public string Value { get; set; }
}

/// <summary>
/// Represents a sub-group of translatable strings within a category (e.g., by field type).
/// </summary>
public class TranslatableStringSubGroupViewModel
{
    /// <summary>
    /// The sub-group name (e.g., field type name).
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The translatable strings in this sub-group.
    /// </summary>
    public IList<TranslatableStringViewModel> Strings { get; set; } = [];
}

/// <summary>
/// Represents a group of translatable strings from a single provider/category.
/// </summary>
public class TranslatableStringGroupViewModel
{
    /// <summary>
    /// The primary category name (e.g., "Permissions", "Content Types", "Content Fields").
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The sub-groups within this category (e.g., grouped by field type).
    /// </summary>
    public IList<TranslatableStringSubGroupViewModel> SubGroups { get; set; } = [];

    /// <summary>
    /// The translatable strings directly in this group (not in sub-groups).
    /// </summary>
    public IList<TranslatableStringViewModel> Strings { get; set; } = [];
}

/// <summary>
/// ViewModel for the translation editor page.
/// </summary>
public class TranslationEditorViewModel
{
    /// <summary>
    /// The currently selected culture.
    /// </summary>
    public string CurrentCulture { get; set; }

    /// <summary>
    /// List of cultures the current user is allowed to edit.
    /// </summary>
    public IList<CultureViewModel> AllowedCultures { get; set; } = [];

    /// <summary>
    /// Whether the current user has read-only access (ViewTranslations only).
    /// </summary>
    public bool IsReadOnly { get; set; }

    /// <summary>
    /// The grouped translatable strings from all providers.
    /// </summary>
    public IList<TranslatableStringGroupViewModel> Providers { get; set; } = [];
}

/// <summary>
/// Represents a culture available for translation.
/// </summary>
public class CultureViewModel
{
    /// <summary>
    /// The culture name (e.g., "fr-FR").
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The display name of the culture (e.g., "French (France)").
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// Whether the current user can edit translations for this culture.
    /// </summary>
    public bool CanEdit { get; set; }
}

/// <summary>
/// Model for saving translations.
/// </summary>
public class TranslationUpdateModel
{
    /// <summary>
    /// The target culture for the translations.
    /// </summary>
    public string Culture { get; set; }

    /// <summary>
    /// The translations to save.
    /// </summary>
    public IList<TranslatableStringViewModel> Translations { get; set; } = [];
}
