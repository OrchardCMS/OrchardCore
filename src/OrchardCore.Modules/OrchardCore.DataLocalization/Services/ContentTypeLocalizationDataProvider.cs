using System.Collections.Generic;
using System.Linq;
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
    
    public IEnumerable<DataLocalizedString> GetDescriptors() => _contentDefinitionService.GetTypesAsync()
        .GetAwaiter()
        .GetResult()
        .Select(t => new DataLocalizedString(_contentTypesContext, t.DisplayName));
}
