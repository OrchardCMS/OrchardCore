using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy;

namespace OrchardCore.Placements.Services
{
    public interface IPlacementFileStore
    {
        /// <summary>
        /// Loads site placement file
        /// </summary>
        Task<PlacementFile> LoadPlacementFileAsync();

        /// <summary>
        /// Stores site placement file
        /// </summary>
        Task SavePlacementFileAsync(PlacementFile placementFile);
    }
}
