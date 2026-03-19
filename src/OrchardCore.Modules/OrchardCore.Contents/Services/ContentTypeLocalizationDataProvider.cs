using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Localization.Data;

namespace OrchardCore.Contents.Services;

public class ContentTypeDataLocalizationProvider : ILocalizationDataProvider
{
    private readonly IContentDefinitionManager _contentDefinitionManager;

    public ContentTypeDataLocalizationProvider(IContentDefinitionManager contentDefinitionManager)
    {
        _contentDefinitionManager = contentDefinitionManager;
    }

    public async Task<IEnumerable<DataLocalizedString>> GetDescriptorsAsync()
        => (await _contentDefinitionManager.ListTypeDefinitionsAsync())
            .Select(t => new DataLocalizedString(OrchardCoreConstants.DataLocalizationContext.ContentTypes, t.DisplayName, string.Empty));
}
