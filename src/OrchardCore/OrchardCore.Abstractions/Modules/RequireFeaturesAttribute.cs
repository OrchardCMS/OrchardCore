using System.Reflection;

namespace OrchardCore.Modules;

/// <summary>
/// When used on a class, it will include the service only
/// if the specific features are enabled.
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

    public static IList<string> GetRequiredFeatureNamesForType(Type type)
    {
        var attribute = type.GetCustomAttributes<RequireFeaturesAttribute>(false).FirstOrDefault();
        return attribute?.RequiredFeatureNames ?? Array.Empty<string>();
    }
}
