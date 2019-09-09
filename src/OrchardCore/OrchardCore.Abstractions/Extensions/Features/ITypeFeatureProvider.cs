using System;
using OrchardCore.Environment.Extensions.Features;

namespace OrchardCore.Environment.Extensions
{
    /// <summary>
    /// An implementation of this service is able to provide the <see cref="Feature"/> that
    /// any services was harvested from.
    /// </summary>
    public interface ITypeFeatureProvider
    {
        /// <summary>
        /// Retrieves the feature that is tied to a given type.
        /// </summary>
        IFeatureInfo GetFeatureForDependency(Type dependency);

        /// <summary>
        /// Ties a given type to a given feature. Note: The last registration wins.
        /// </summary>
        void Add(Type type, IFeatureInfo feature);
    }
}
