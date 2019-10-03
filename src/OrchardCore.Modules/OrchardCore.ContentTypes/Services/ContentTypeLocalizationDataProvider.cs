using System.Collections.Generic;
using System.Linq;
using OrchardCore.Localization;

namespace OrchardCore.ContentTypes.Services
{
    public class ContentTypeDataLocalizationProvider : ILocalizationDataProvider
    {
        private readonly IContentDefinitionService _contentDefinitionService;

        public ContentTypeDataLocalizationProvider(IContentDefinitionService contentDefinitionService)
        {
            _contentDefinitionService = contentDefinitionService;
        }
        
        public IEnumerable<string> GetAllStrings()
            => _contentDefinitionService.GetTypes().Select(t => t.DisplayName);
    }
}