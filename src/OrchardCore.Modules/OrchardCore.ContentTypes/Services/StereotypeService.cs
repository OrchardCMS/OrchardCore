using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Modules;

namespace OrchardCore.ContentTypes.Services
{
    public class StereotypeService : IStereotypeService
    {
        private readonly IEnumerable<IStereotypesProvider> _providers;
        private readonly IContentDefinitionService _contentDefinitionService;
        private readonly ILogger<StereotypeService> _logger;

        public StereotypeService(
            IEnumerable<IStereotypesProvider> providers,
            IContentDefinitionService contentDefinitionService,
            ILogger<StereotypeService> logger)
        {
            _providers = providers;
            _contentDefinitionService = contentDefinitionService;
            _logger = logger;
        }

        public async Task<IEnumerable<StereotypeDescription>> GetStereotypesAsync()
        {
            var stereotypesInProvider = await _providers.InvokeAsync(provider => provider.GetStereotypesAsync(), _logger);

            var stereotypesInType = (await _contentDefinitionService.GetTypesAsync())
                .Select(contentType => contentType.TypeDefinition.TryGetStereotype(out var stereotype) ? stereotype : null)
                .Where(stereotype => stereotype != null && stereotypesInProvider.Any(x => x.Stereotype == stereotype))
                .ToHashSet();

            return stereotypesInProvider.Union(stereotypesInType.Select(x => new StereotypeDescription { Stereotype = x, DisplayName = x }));
        }
    }
}
