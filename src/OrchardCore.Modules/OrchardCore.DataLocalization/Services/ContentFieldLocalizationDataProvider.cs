using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Localization.Data;

namespace OrchardCore.DataLocalization.Services;

public class ContentFieldDataLocalizationProvider : ILocalizationDataProvider
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    private static readonly string _contentFieldsContext = "Content Fields";

    public ContentFieldDataLocalizationProvider(IContentDefinitionManager contentDefinitionManager)
    {
        _contentDefinitionManager = contentDefinitionManager;
    }

    // TODO: Check if there's a better way to get the fields
    public async Task<IEnumerable<DataLocalizedString>> GetDescriptorsAsync()
        => (await _contentDefinitionManager.ListTypeDefinitionsAsync())
            .SelectMany(t => t.Parts)
            .SelectMany(p => p.PartDefinition.Fields.Select(f => new DataLocalizedString(_contentFieldsContext, f.Name, string.Empty)));
}
