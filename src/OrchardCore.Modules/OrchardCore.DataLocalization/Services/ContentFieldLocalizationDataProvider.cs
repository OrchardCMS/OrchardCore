using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentTypes.Services;
using OrchardCore.Localization.Data;

namespace OrchardCore.DataLocalization.Services;

public class ContentFieldDataLocalizationProvider : ILocalizationDataProvider
{
    private readonly IContentDefinitionService _contentDefinitionService;

    private static readonly string _contentFieldsContext = "Content Fields";

    public ContentFieldDataLocalizationProvider(IContentDefinitionService contentDefinitionService)
    {
        _contentDefinitionService = contentDefinitionService;
    }

    // TODO: Check if there's a better way to get the fields
    public async Task<IEnumerable<DataLocalizedString>> GetDescriptorsAsync()
        => (await _contentDefinitionService.GetTypesAsync())
            .SelectMany(t => t.TypeDefinition.Parts)
            .SelectMany(p => p.PartDefinition.Fields.Select(f => new DataLocalizedString(_contentFieldsContext, f.Name, string.Empty)));
}
