using System.Collections.Generic;
using System.Linq;
using OrchardCore.Localization;

namespace OrchardCore.ContentTypes.Services
{
    public class ContentFieldDataLocalizationProvider : ILocalizationDataProvider
    {
        private readonly IContentDefinitionService _contentDefinitionService;

        public ContentFieldDataLocalizationProvider(IContentDefinitionService contentDefinitionService)
        {
            _contentDefinitionService = contentDefinitionService;
        }
        
        // TODO: Check if there's a better way to get the fields
        public IEnumerable<string> GetAllStrings()
            => _contentDefinitionService.GetTypes()
                    .SelectMany(t => t.TypeDefinition.Parts)
                    .Where(p => p.PartDefinition.Fields.Count() > 0)
                    .SelectMany(p => p.PartDefinition.Fields.Select(f => f.Name));
    }
}