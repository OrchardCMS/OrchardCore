using System.Threading.Tasks;
using OrchardCore.Placements.Models;

namespace OrchardCore.Placements.Services
{
    public interface IPlacementStore
    {
        /// <summary>
        /// Loads the placements document from the store for updating and that should not be cached.
        /// </summary>
        Task<PlacementsDocument> LoadPlacementsAsync();

        /// <summary>
        /// Gets the placements document from the cache for sharing and that should not be updated.
        /// </summary>
        Task<PlacementsDocument> GetPlacementsAsync();

        /// <summary>
        /// Updates the store with the provided placements document and then updates the cache.
        /// </summary>
        Task SavePlacementsAsync(PlacementsDocument placementsDocument);
    }
}
