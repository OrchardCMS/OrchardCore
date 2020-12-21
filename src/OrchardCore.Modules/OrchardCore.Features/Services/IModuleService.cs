using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Features.Models;

namespace OrchardCore.Features.Services
{
    public interface IModuleService
    {
        /// <summary>
        /// Retrieves an enumeration of the available features together with its state (enabled / disabled).
        /// </summary>
        /// <returns>An enumeration of the available features together with its state (enabled / disabled).</returns>
        Task<IEnumerable<ModuleFeature>> GetAvailableFeaturesAsync();

        /// <summary>
        /// Enables a list of features.
        /// </summary>
        /// <param name="featureIds">The IDs for the features to be enabled.</param>
        Task EnableFeaturesAsync(IEnumerable<string> featureIds);

        /// <summary>
        /// Enables a list of features.
        /// </summary>
        /// <param name="featureIds">The IDs for the features to be enabled.</param>
        /// <param name="force">Boolean parameter indicating if the feature should enable it's dependencies if required or fail otherwise.</param>
        Task EnableFeaturesAsync(IEnumerable<string> featureIds, bool force);

        /// <summary>
        /// Disables a list of features.
        /// </summary>
        /// <param name="featureIds">The IDs for the features to be disabled.</param>
        Task DisableFeaturesAsync(IEnumerable<string> featureIds);

        /// <summary>
        /// Disables a list of features.
        /// </summary>
        /// <param name="featureIds">The IDs for the features to be disabled.</param>
        /// <param name="force">Boolean parameter indicating if the feature should disable the features which depend on it if required or fail otherwise.</param>
        Task DisableFeaturesAsync(IEnumerable<string> featureIds, bool force);
    }
}
