using System.Reflection;

namespace OrchardCore.Modules;

/// <summary>
/// When used on a class, it includes the service only if the specified features are enabled.
/// An explicitly empty declaration, <c>[RequireFeatures()]</c>, composes the service for every tenant.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class RequireFeaturesAttribute : Attribute
{
    public RequireFeaturesAttribute(params string[] featureNames)
    {
        RequiredFeatureNames = featureNames;
    }

    /// <summary>
    /// The names of the required features.
    /// </summary>
    public IList<string> RequiredFeatureNames { get; }

    /// <summary>
    /// Gets a value indicating whether the attributed type is composed for every tenant.
    /// </summary>
    public bool IsAlwaysComposed => RequiredFeatureNames.Count == 0;

    /// <summary>
    /// Gets a value indicating whether the specified type explicitly declares an empty
    /// <see cref="RequireFeaturesAttribute"/> and is therefore composed for every tenant.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    public static bool IsAlwaysComposedForType(Type type)
    {
        return type.GetCustomAttributes<RequireFeaturesAttribute>(false).FirstOrDefault()?.IsAlwaysComposed ?? false;
    }

    /// <summary>
    /// Gets the feature names required by the specified type.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    public static IList<string> GetRequiredFeatureNamesForType(Type type)
    {
        var attribute = type.GetCustomAttributes<RequireFeaturesAttribute>(false).FirstOrDefault();
        return attribute?.RequiredFeatureNames ?? Array.Empty<string>();
    }
}
