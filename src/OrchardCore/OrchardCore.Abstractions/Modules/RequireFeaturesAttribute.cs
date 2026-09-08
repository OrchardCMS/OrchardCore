using System.Reflection;

namespace OrchardCore.Modules;

/// <summary>
/// When used on a class, it includes the service only if the specified features are enabled.
/// Omitting the attribute or declaring it without feature names adds no named feature requirements.
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
    /// Gets the feature names required by the specified type.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    public static IList<string> GetRequiredFeatureNamesForType(Type type)
    {
        var attribute = type.GetCustomAttributes<RequireFeaturesAttribute>(false).FirstOrDefault();
        return attribute?.RequiredFeatureNames ?? Array.Empty<string>();
    }
}
