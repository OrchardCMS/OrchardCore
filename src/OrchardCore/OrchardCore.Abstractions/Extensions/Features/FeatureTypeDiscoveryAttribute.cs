using System.Reflection;

namespace OrchardCore.Environment.Extensions;

/// <summary>
/// Configures how the <see cref="ITypeFeatureProvider" /> will assign the type to features.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public class FeatureTypeDiscoveryAttribute : Attribute
{
    /// <summary>
    /// Prevents assignment of a public type to the main feature.
    /// </summary>
    /// <remarks>
    /// If <c>SkipExtension</c> is set to true, the type is only added to the same feature
    /// as its startup class. 
    /// </remarks>
    public bool SkipExtension { get; set; }

    /// <summary>
    /// Ensures a type is only registered with a single feature.
    /// </summary>
    /// <remarks>
    /// If <c>SingleFeatureOnly</c> is set to true, the <c>TypeFeatureProvider</c> will throw
    /// an <c>InvalidOperationException</c> if the type gets assigned to more than one feature.
    /// </remarks>
    public bool SingleFeatureOnly { get; set; }

    public static FeatureTypeDiscoveryAttribute GetFeatureTypeDiscoveryForType(Type type)
    {
        return type.GetCustomAttribute<FeatureTypeDiscoveryAttribute>(true)
            ?? type.GetInterfaces()
                    .Select(dmType => dmType.GetCustomAttribute<FeatureTypeDiscoveryAttribute>(true))
                    .FirstOrDefault(featureTypeDiscoveryAttr => featureTypeDiscoveryAttr != null);
    }
}
