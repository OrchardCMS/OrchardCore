using System;
using System.Collections.Generic;
using OrchardCore.Environment.Extensions.Features;

namespace OrchardCore.Environment.Extensions
{
    /// <summary>
    /// An implementation of this service is able to provide the <see cref="IFeatureInfo"/> that
    /// any services was harvested from.
    /// </summary>
    public interface ITypeFeatureProvider
    {
        /// <summary>
        /// Gets the extension for the specified dependent type.
        /// </summary>
        /// <param name="dependency"></param>
        /// <returns></returns>
        IExtensionInfo GetExtensionForDependency(Type dependency);

        /// <summary>
        /// Gets the first feature for the specified dependent type.
        /// </summary>
        /// <param name="dependency"></param>
        /// <returns></returns>
        IFeatureInfo GetFeatureForDependency(Type dependency);

        /// <summary>
        /// Gets all features for the specified dependent type.
        /// </summary>
        /// <param name="dependency"></param>
        /// <returns></returns>
        IEnumerable<IFeatureInfo> GetFeaturesForDependency(Type dependency);

        /// <summary>
        /// Gets all dependent types for the specified feature.
        /// </summary>
        /// <param name="feature"></param>
        /// <returns></returns>
        IEnumerable<Type> GetTypesForFeature(IFeatureInfo feature);

        /// <summary>
        /// Adds a type to the specified feature.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="feature"></param>
        void TryAdd(Type type, IFeatureInfo feature);
    }
}
