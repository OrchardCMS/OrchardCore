using Orchard.DependencyInjection;
using Orchard.Environment.Extensions.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orchard.Environment.Extensions.Features
{
    public delegate void FeatureDependencyNotificationHandler(string messageFormat, string featureId, IEnumerable<string> featureIds);

    public interface IFeatureManager
    {
        FeatureDependencyNotificationHandler FeatureDependencyNotification { get; set; }

        /// <summary>
        /// Retrieves the available features.
        /// </summary>
        /// <returns>An enumeration of feature descriptors for the available features.</returns>
        Task<IEnumerable<FeatureDescriptor>> GetAvailableFeaturesAsync();

        /// <summary>
        /// Retrieves the enabled features.
        /// </summary>
        /// <returns>An enumeration of feature descriptors for the enabled features.</returns>
        Task<IEnumerable<FeatureDescriptor>> GetEnabledFeaturesAsync();

        /// <summary>
        /// Retrieves the disabled features.
        /// </summary>
        /// <returns>An enumeration of feature descriptors for the disabled features.</returns>
        Task<IEnumerable<FeatureDescriptor>> GetDisabledFeaturesAsync();

        /// <summary>
        /// Enables a list of features.
        /// </summary>
        /// <param name="featureIds">The IDs for the features to be enabled.</param>
        /// <returns>An enumeration with the enabled feature IDs.</returns>
        Task<IEnumerable<string>> EnableFeaturesAsync(IEnumerable<string> featureIds);

        /// <summary>
        /// Enables a list of features.
        /// </summary>
        /// <param name="featureIds">The IDs for the features to be enabled.</param>
        /// <param name="force">Boolean parameter indicating if the feature should enable it's dependencies if required or fail otherwise.</param>
        /// <returns>An enumeration with the enabled feature IDs.</returns>
        Task<IEnumerable<string>> EnableFeaturesAsync(IEnumerable<string> featureIds, bool force);

        /// <summary>
        /// Disables a list of features.
        /// </summary>
        /// <param name="featureIds">The IDs for the features to be disabled.</param>
        /// <returns>An enumeration with the disabled feature IDs.</returns>
        Task<IEnumerable<string>> DisableFeaturesAsync(IEnumerable<string> featureIds);

        /// <summary>
        /// Disables a list of features.
        /// </summary>
        /// <param name="featureIds">The IDs for the features to be disabled.</param>
        /// <param name="force">Boolean parameter indicating if the feature should disable the features which depend on it if required or fail otherwise.</param>
        /// <returns>An enumeration with the disabled feature IDs.</returns>
        Task<IEnumerable<string>> DisableFeaturesAsync(IEnumerable<string> featureIds, bool force);

        /// <summary>
        /// Lists all enabled features that depend on a given feature.
        /// </summary>
        /// <param name="featureId">ID of the feature to check.</param>
        /// <returns>An enumeration with dependent feature IDs.</returns>
        Task<IEnumerable<string>> GetDependentFeaturesAsync(string featureId);
    }
}