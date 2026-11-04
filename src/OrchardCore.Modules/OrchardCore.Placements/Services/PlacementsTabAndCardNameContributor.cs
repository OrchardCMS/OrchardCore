using OrchardCore.DisplayManagement.Descriptors;

namespace OrchardCore.Placements.Services;

/// <summary>
/// Lists the tab and card names of the stored placement rules, whatever their filters.
/// </summary>
internal sealed class PlacementsTabAndCardNameContributor : ITabAndCardNameContributor
{
    private readonly PlacementsManager _placementsManager;

    public PlacementsTabAndCardNameContributor(PlacementsManager placementsManager)
    {
        _placementsManager = placementsManager;
    }

    public async Task<IEnumerable<string>> GetNamesAsync()
    {
        var placements = await _placementsManager.ListShapePlacementsAsync();

        return placements.Values
            .SelectMany(nodes => nodes ?? [])
            .Where(node => !string.IsNullOrEmpty(node.Location))
            .SelectMany(node => TabAndCardNames.FromLocation(node.Location))
            .ToArray();
    }
}
