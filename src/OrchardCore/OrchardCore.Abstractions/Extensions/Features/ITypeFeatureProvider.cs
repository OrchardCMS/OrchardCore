using System;
using OrchardCore.Environment.Extensions.Features;

namespace OrchardCore.Environment.Extensions
{
    /// <summary>
    /// An implementation of this service is able to provide the <see cref="IFeatureInfo"/> that
    /// any services was harvested from.
    /// </summary>
    public interface ITypeFeatureProvider
    {
        IFeatureInfo GetFeatureForDependency(Type dependency);
        void TryAdd(Type type, IFeatureInfo feature);
    }
}
