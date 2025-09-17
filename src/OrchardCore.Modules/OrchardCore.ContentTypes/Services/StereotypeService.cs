using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Modules;

namespace OrchardCore.ContentTypes.Services;

public class StereotypeService : IStereotypeService
{
    private readonly IEnumerable<IStereotypesProvider> _providers;
    private readonly IContentDefinitionViewModelService _contentDefinitionViewModelService;
    private readonly ILogger<StereotypeService> _logger;

    public StereotypeService(
        IEnumerable<IStereotypesProvider> providers,
        ILogger<StereotypeService> logger,
        IContentDefinitionViewModelService contentDefinitionViewModelService)
    {
        _providers = providers;
        _logger = logger;
        _contentDefinitionViewModelService = contentDefinitionViewModelService;
    }

    public async Task<IEnumerable<StereotypeDescription>> GetStereotypesAsync()
    {
        var providerStereotypes = (await _providers.InvokeAsync(provider => provider.GetStereotypesAsync(), _logger)).ToList();

        var stereotypes = providerStereotypes.Select(providerStereotype => providerStereotype.Stereotype)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var contentType in await _contentDefinitionViewModelService.GetTypesAsync())
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
