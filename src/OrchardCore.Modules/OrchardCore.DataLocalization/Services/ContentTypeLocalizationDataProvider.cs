using OrchardCore.ContentTypes.Services;
using OrchardCore.Localization.Data;

namespace OrchardCore.DataLocalization.Services;

public class ContentTypeDataLocalizationProvider : ILocalizationDataProvider
{
    private readonly IContentDefinitionViewModelService _contentDefinitionAppService;

    private static readonly string _contentTypesContext = "Content Types";

    public ContentTypeDataLocalizationProvider(IContentDefinitionViewModelService contentDefinitionAppService)
    {
        _contentDefinitionAppService = contentDefinitionAppService;
    }

    public async Task<IEnumerable<DataLocalizedString>> GetDescriptorsAsync()
        => (await _contentDefinitionAppService.GetTypesAsync())
            .Select(t => new DataLocalizedString(_contentTypesContext, t.DisplayName, string.Empty));
}
