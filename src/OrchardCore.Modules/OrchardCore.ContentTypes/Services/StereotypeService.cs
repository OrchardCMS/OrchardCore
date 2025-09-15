using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Modules;

namespace OrchardCore.ContentTypes.Services;

public class StereotypeService : IStereotypeService
{
    private readonly IEnumerable<IStereotypesProvider> _providers;
    private readonly IContentDefinitionViewModelService _contentDefinitionAppService;
    private readonly ILogger<StereotypeService> _logger;

    public StereotypeService(
        IEnumerable<IStereotypesProvider> providers,
        ILogger<StereotypeService> logger,
        IContentDefinitionViewModelService contentDefinitionAppService)
    {
        _providers = providers;
        _logger = logger;
        _contentDefinitionAppService = contentDefinitionAppService;
    }

    public async Task<IEnumerable<StereotypeDescription>> GetStereotypesAsync()
    {
        var providerStereotypes = (await _providers.InvokeAsync(provider => provider.GetStereotypesAsync(), _logger)).ToList();

        var stereotypes = providerStereotypes.Select(providerStereotype => providerStereotype.Stereotype)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var contentType in await _contentDefinitionAppService.GetTypesAsync())
        {
            if (!contentType.TypeDefinition.TryGetStereotype(out var stereotype) ||
                stereotypes.Contains(stereotype))
            {
                continue;
            }

            providerStereotypes.Add(new StereotypeDescription
            {
                Stereotype = stereotype,
                DisplayName = stereotype,
            });
        }

        return providerStereotypes.OrderBy(x => x.DisplayName);
    }
}
