using System.Collections.Generic;
using System.Linq;
using OrchardCore.ContentTypes.Services;

namespace OrchardCore.Localization.Dynamic.Services
{
    public class ContentTypeDataLocalizationProvider : ILocalizationDataProvider
    {
        private readonly IContentDefinitionService _contentDefinitionService;

        private static readonly string ContentTypesContext = "Content Types";

        public ContentTypeDataLocalizationProvider(IContentDefinitionService contentDefinitionService)
        {
            _contentDefinitionService = contentDefinitionService;
        }
        
        public IEnumerable<DataLocalizedString> GetAllStrings()
            => _contentDefinitionService.GetTypes().Select(t => new DataLocalizedString(ContentTypesContext, t.DisplayName));
    }
}
