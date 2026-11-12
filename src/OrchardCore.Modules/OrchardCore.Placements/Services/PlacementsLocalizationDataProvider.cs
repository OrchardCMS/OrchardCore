using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.Localization.Data;

namespace OrchardCore.Placements.Services;

/// <summary>
/// Provides the tab and card names of the stored placement rules, whatever their filters.
/// </summary>
internal sealed class PlacementsLocalizationDataProvider : ILocalizationDataProvider
{
    private readonly PlacementsManager _placementsManager;

    public PlacementsLocalizationDataProvider(PlacementsManager placementsManager)
    {
        _placementsManager = placementsManager;
    }

    public async Task<IEnumerable<DataLocalizedString>> GetDescriptorsAsync()
    {
        var placements = await _placementsManager.ListShapePlacementsAsync();

        return placements.Values
            .SelectMany(nodes => nodes ?? [])
            .Where(node => !string.IsNullOrEmpty(node.Location))
            .SelectMany(node => TabAndCardNames.FromLocation(node.Location))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(name => new DataLocalizedString(TabAndCardNames.DataLocalizationContext, name, string.Empty))
            .ToArray();
    }
}
