using OrchardCore.ContentTypes.Services;
using OrchardCore.Localization.Data;

namespace OrchardCore.DataLocalization.Services;

public class ContentTypeDataLocalizationProvider : ILocalizationDataProvider
{
    private readonly IContentDefinitionViewModelService _contentDefinitionViewModelService;

    private static readonly string _contentTypesContext = "Content Types";

    public ContentTypeDataLocalizationProvider(IContentDefinitionViewModelService contentDefinitionViewModelService)
    {
        _contentDefinitionViewModelService = contentDefinitionViewModelService;
    }

    public async Task<IEnumerable<DataLocalizedString>> GetDescriptorsAsync()
        => (await _contentDefinitionViewModelService.GetTypesAsync())
            .Select(t => new DataLocalizedString(_contentTypesContext, t.DisplayName, string.Empty));
}
