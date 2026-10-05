using OrchardCore.DataLocalization.Models;

namespace OrchardCore.DataLocalization.ViewModels;

public sealed class UiTranslationsViewModel
{
    public string Culture { get; set; }
    public string Search { get; set; }
    public string AssemblyName { get; set; }
    public string Status { get; set; }
    public string[] Cultures { get; set; } = [];
    public string[] Assemblies { get; set; } = [];
    public IReadOnlyList<UiLocalizationResource> Resources { get; set; } = [];
    public IReadOnlyList<UiTranslation> Overrides { get; set; } = [];
    public int PluralFormCount { get; set; }
    public bool CanEdit { get; set; }
    public dynamic Pager { get; set; }
}
