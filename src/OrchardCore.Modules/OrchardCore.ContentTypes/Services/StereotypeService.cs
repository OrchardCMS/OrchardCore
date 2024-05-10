using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentTypes.Services
{
    public class StereotypeService : IStereotypeService
    {
        private readonly IEnumerable<IStereotypesProvider> _providers;
        private readonly IContentDefinitionService _contentDefinitionService;
        public StereotypeService(IEnumerable<IStereotypesProvider> providers, IContentDefinitionService contentDefinitionService)
        {
            _providers = providers;
            _contentDefinitionService = contentDefinitionService;
        }

        public async Task<IEnumerable<StereotypeDescription>> GetStereotypesAsync()
        {
            var descriptions = new List<StereotypeDescription>();

            foreach (var provider in _providers)
            {
                descriptions.AddRange(await provider.GetStereotypesAsync());
            }
            var stereotypes = (await _contentDefinitionService.GetTypesAsync())
                .Select(x => x.Settings["Stereotype"]?.ToString())
                .Where(x => x != null)
                .Distinct()
                .Except(descriptions.Select(d => d.Stereotype));

            return descriptions;
        }
    }
}
