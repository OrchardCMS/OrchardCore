namespace OrchardCore.DataLocalization.ViewModels;

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
