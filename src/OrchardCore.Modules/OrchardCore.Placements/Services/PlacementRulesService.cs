using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy;
using OrchardCore.Environment.Cache;

namespace OrchardCore.Placements.Services
{
    public class PlacementRulesService : IPlacementRulesService
    {
        private const string CacheKey = nameof(PlacementRulesService);

        private readonly ISignal _signal;
        private readonly IPlacementFileStore _placementFileStore;
        private readonly IMemoryCache _memoryCache;

        public PlacementRulesService(
            ISignal signal,
            IPlacementFileStore placementFileStore,
            IMemoryCache memoryCache)
        {
            _signal = signal;
            _placementFileStore = placementFileStore;
            _memoryCache = memoryCache;
        }

        public IChangeToken ChangeToken => _signal.GetToken(CacheKey);

        public PlacementNode[] GetShapePlacements(string shapeType)
        {
            var placementFile = GetPlacementFile();

            if (placementFile.ContainsKey(shapeType))
            {
                return placementFile[shapeType];
            }
            else
            {
                return new PlacementNode[0];
            }
        }

        public IDictionary<string, PlacementNode[]> ListShapePlacements()
        {
            return GetPlacementFile();
        }

        public async Task UpdateShapePlacementsAsync(string shapeType, PlacementNode[] placementNodes)
        {
            var placementFile = GetPlacementFile();

            placementFile[shapeType] = placementNodes;

            _memoryCache.Set(CacheKey, placementFile);
            _signal.DeferredSignalToken(CacheKey);

            await _placementFileStore.SavePlacementFileAsync(placementFile);
        }

        public async Task RemoveShapePlacementsAsync(string shapeType)
        {
            var placementFile = GetPlacementFile();

            if (placementFile.ContainsKey(shapeType))
            {
                placementFile.Remove(shapeType);

                _memoryCache.Set(CacheKey, placementFile);
                _signal.DeferredSignalToken(CacheKey);

                await _placementFileStore.SavePlacementFileAsync(placementFile);
            }
        }

        private PlacementFile GetPlacementFile()
        {
            if (!_memoryCache.TryGetValue<PlacementFile>(CacheKey, out var placementFile))
            {
                var changeToken = ChangeToken;

                placementFile = _placementFileStore.LoadPlacementFileAsync().GetAwaiter().GetResult();

                _memoryCache.Set(CacheKey, placementFile, changeToken);
            }

            return placementFile;
        }
    }
}
