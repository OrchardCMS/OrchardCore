using Orchard.DependencyInjection;
using Orchard.Environment.Extensions.Models;
using System.Collections.Generic;

namespace Orchard.Environment.Extensions.Features
{
    public delegate void FeatureDependencyNotificationHandler(string messageFormat, string featureId, IEnumerable<string> featureIds);

    public interface IFeatureManager : IDependency
    {
        FeatureDependencyNotificationHandler FeatureDependencyNotification { get; set; }

        /// <summary>
        /// Retrieves the available features.
        /// </summary>
        /// <returns>An enumeration of feature descriptors for the available features.</returns>
        IEnumerable<FeatureDescriptor> GetAvailableFeatures();

        /// <summary>
        /// Retrieves the enabled features.
        /// </summary>
        /// <returns>An enumeration of feature descriptors for the enabled features.</returns>
        IEnumerable<FeatureDescriptor> GetEnabledFeatures();

        /// <summary>
        /// Retrieves the disabled features.
        /// </summary>
        /// <returns>An enumeration of feature descriptors for the disabled features.</returns>
        IEnumerable<FeatureDescriptor> GetDisabledFeatures();

        /// <summary>
        /// Enables a list of features.
        /// </summary>
        /// <param name="featureIds">The IDs for the features to be enabled.</param>
        /// <returns>An enumeration with the enabled feature IDs.</returns>
        IEnumerable<string> EnableFeatures(IEnumerable<string> featureIds);

        /// <summary>
        /// Enables a list of features.
        /// </summary>
        /// <param name="featureIds">The IDs for the features to be enabled.</param>
        /// <param name="force">Boolean parameter indicating if the feature should enable it's dependencies if required or fail otherwise.</param>
        /// <returns>An enumeration with the enabled feature IDs.</returns>
        IEnumerable<string> EnableFeatures(IEnumerable<string> featureIds, bool force);

        /// <summary>
        /// Disables a list of features.
        /// </summary>
        /// <param name="featureIds">The IDs for the features to be disabled.</param>
        /// <returns>An enumeration with the disabled feature IDs.</returns>
        IEnumerable<string> DisableFeatures(IEnumerable<string> featureIds);

        /// <summary>
        /// Disables a list of features.
        /// </summary>
        /// <param name="featureIds">The IDs for the features to be disabled.</param>
        /// <param name="force">Boolean parameter indicating if the feature should disable the features which depend on it if required or fail otherwise.</param>
        /// <returns>An enumeration with the disabled feature IDs.</returns>
        IEnumerable<string> DisableFeatures(IEnumerable<string> featureIds, bool force);

        /// <summary>
        /// Lists all enabled features that depend on a given feature.
        /// </summary>
        /// <param name="featureId">ID of the feature to check.</param>
        /// <returns>An enumeration with dependent feature IDs.</returns>
        IEnumerable<string> GetDependentFeatures(string featureId);
    }
}