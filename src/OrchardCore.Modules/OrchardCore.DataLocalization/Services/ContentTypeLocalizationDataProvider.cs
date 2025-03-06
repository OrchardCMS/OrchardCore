using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentTypes.Services;
using OrchardCore.Localization.Data;

namespace OrchardCore.DataLocalization.Services;

public class ContentTypeDataLocalizationProvider : ILocalizationDataProvider
{
    private readonly IContentDefinitionService _contentDefinitionService;

    private static readonly string _contentTypesContext = "Content Types";

    public ContentTypeDataLocalizationProvider(IContentDefinitionService contentDefinitionService)
    {
        _contentDefinitionService = contentDefinitionService;
    }
    
    public async Task<IEnumerable<DataLocalizedString>> GetDescriptorsAsync()
        => (await _contentDefinitionService.GetTypesAsync())
            .Select(t => new DataLocalizedString(_contentTypesContext, t.DisplayName, string.Empty));
}
