using OrchardCore.DataLocalization.Models;

namespace OrchardCore.DataLocalization.ViewModels;

/// <summary>
/// A batch of changed UI overrides for a single configured culture.
/// </summary>
public sealed class UiTranslationUpdateModel
{
    /// <summary>Gets or sets the target culture.</summary>
    public string Culture { get; set; }

    /// <summary>Gets or sets changed entries. Empty values arrays remove overrides.</summary>
    public UiTranslation[] Translations { get; set; }
}
