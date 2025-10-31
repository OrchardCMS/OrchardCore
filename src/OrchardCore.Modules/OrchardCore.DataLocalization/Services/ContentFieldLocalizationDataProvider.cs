using OrchardCore.ContentTypes.Services;
using OrchardCore.Localization.Data;

namespace OrchardCore.DataLocalization.Services;

public class ContentFieldDataLocalizationProvider : ILocalizationDataProvider
{
    private readonly IContentDefinitionViewModelService _contentDefinitionViewModelService;

    private static readonly string _contentFieldsContext = "Content Fields";

    public ContentFieldDataLocalizationProvider(IContentDefinitionViewModelService contentDefinitionViewModelService)
    {
        _contentDefinitionViewModelService = contentDefinitionViewModelService;
    }

    // TODO: Check if there's a better way to get the fields
    public async Task<IEnumerable<DataLocalizedString>> GetDescriptorsAsync()
        => (await _contentDefinitionViewModelService.GetTypesAsync())
            .SelectMany(t => t.TypeDefinition.Parts)
            .SelectMany(p => p.PartDefinition.Fields.Select(f => new DataLocalizedString(_contentFieldsContext, f.Name, string.Empty)));
}
