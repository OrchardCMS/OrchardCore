using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Localization.Data;

namespace OrchardCore.DataLocalization.Services;

public class ContentFieldDataLocalizationProvider : ILocalizationDataProvider
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public ContentFieldDataLocalizationProvider(IContentDefinitionManager contentDefinitionManager)
    {
        _contentDefinitionManager = contentDefinitionManager;
    }

    public async Task<IEnumerable<DataLocalizedString>> GetDescriptorsAsync()
    {
        var typeDefinitions = await _contentDefinitionManager.ListTypeDefinitionsAsync();

        // Use the field's DisplayName as the key with a context based on PartName.FieldDisplayName.
        // This ensures uniqueness when the same part has multiple fields of the same type.
        return typeDefinitions
            .SelectMany(t => t.Parts)
            .SelectMany(p => p.PartDefinition.Fields.Select(f =>
                new DataLocalizedString(
                    $"{p.PartDefinition.Name}.{f.DisplayName()}",
                    f.DisplayName(),
                    string.Empty)))
            .DistinctBy(d => $"{d.Context}|{d.Name}");
    }
}
