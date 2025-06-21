using OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy;

namespace OrchardCore.Placements.Services;

public sealed class PlacementsManager
{
    private readonly IPlacementStore _placementStore;

    public PlacementsManager(IPlacementStore placementStore)
    {
        _placementStore = placementStore;
    }

    public async Task<IReadOnlyDictionary<string, PlacementNode[]>> ListShapePlacementsAsync()
    {
        var document = await _placementStore.GetPlacementsAsync().ConfigureAwait(false);

        return document.Placements;
    }

    public async Task<PlacementNode[]> GetShapePlacementsAsync(string shapeType)
    {
        var document = await _placementStore.GetPlacementsAsync().ConfigureAwait(false);

        if (document.Placements.TryGetValue(shapeType, out var nodes))
        {
            return nodes;
        }
        else
        {
            return null;
        }
    }

    public async Task UpdateShapePlacementsAsync(string shapeType, IEnumerable<PlacementNode> placementNodes)
    {
        var document = await _placementStore.LoadPlacementsAsync().ConfigureAwait(false);

        document.Placements[shapeType] = placementNodes.ToArray();

        await _placementStore.SavePlacementsAsync(document).ConfigureAwait(false);
    }

    public async Task RemoveShapePlacementsAsync(string shapeType)
    {
        var document = await _placementStore.LoadPlacementsAsync().ConfigureAwait(false);

        if (document.Placements.Remove(shapeType))
        {
            await _placementStore.SavePlacementsAsync(document).ConfigureAwait(false);
        }
    }
}
