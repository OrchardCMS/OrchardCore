using OrchardCore.Environment.Extensions.Features;

namespace OrchardCore.Environment.Extensions;

/// <summary>
/// An implementation of this service is able to provide the <see cref="IFeatureInfo"/> that
/// any services was harvested from.
/// </summary>
public interface ITypeFeatureProvider
{
    /// <summary>
    /// Gets the extension for the specified dependent type.
    /// </summary>
    IExtensionInfo GetExtensionForDependency(Type dependency);

    /// <summary>
    /// Gets the first feature for the specified dependent type.
    /// </summary>
    /// <remarks>
    /// If a type has been registered for more than one feature,
    /// <see cref="GetFeatureForDependency"/> returns the first feature that has
    /// the same ID as the parent extension.
    /// Use this method when you only need one feature of a module, such
    /// as when applying migrations for the entire module as opposed to
    /// functionality of individual features.
    /// </remarks>
    IFeatureInfo GetFeatureForDependency(Type dependency);

    /// <summary>
    /// Gets all features for the specified dependent type.
    /// </summary>
    IEnumerable<IFeatureInfo> GetFeaturesForDependency(Type dependency);

    /// <summary>
    /// Gets all dependent types for the specified feature.
    /// </summary>
    IEnumerable<Type> GetTypesForFeature(IFeatureInfo feature);

    /// <summary>
    /// Adds a type to the specified feature.
    /// </summary>
    void TryAdd(Type type, IFeatureInfo feature);
}
