using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Localization.Data;

namespace OrchardCore.Contents.Services;

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

        // Use "Content Fields" as primary context and part name as sub-context for grouping.
        // Format: "Content Fields:PartName" where PartName is the content part name.
        return typeDefinitions
            .SelectMany(t => t.Parts)
            .SelectMany(p => p.PartDefinition.Fields.Select(f =>
                new DataLocalizedString(
                    $"Content Fields{Constants.ContextSeparator}{p.PartDefinition.Name}",
                    f.DisplayName(),
                    string.Empty)))
            .DistinctBy(d => $"{d.Context}|{d.Name}");
    }
}
