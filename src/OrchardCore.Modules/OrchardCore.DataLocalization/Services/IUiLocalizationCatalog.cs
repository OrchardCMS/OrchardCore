using OrchardCore.DataLocalization.Models;

namespace OrchardCore.DataLocalization.Services;

/// <summary>Provides authoritative UI resources for editors and automated translation workflows.</summary>
public interface IUiLocalizationCatalog
{
    /// <summary>Gets resources, including untranslated entries and extracted metadata.</summary>
    IReadOnlyList<UiLocalizationResource> GetResources();
}
