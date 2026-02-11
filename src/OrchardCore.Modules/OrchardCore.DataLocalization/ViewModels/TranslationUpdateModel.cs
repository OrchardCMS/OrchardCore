namespace OrchardCore.DataLocalization.ViewModels;

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
