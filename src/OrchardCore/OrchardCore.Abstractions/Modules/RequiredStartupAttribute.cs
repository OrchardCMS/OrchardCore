using System.Reflection;

namespace OrchardCore.Modules;

/// <summary>
/// Marks a component type to be composed for every tenant.
/// </summary>
/// <remarks>
/// The marked type and services registered by its startup class are attributed to
/// <see cref="Application.DefaultFeatureId"/>. This attribute can be combined with
/// <see cref="RequireFeaturesAttribute"/>; the type is composed when it is marked required
/// or all named feature requirements are enabled.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class RequiredStartupAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RequiredStartupAttribute"/> class.
    /// </summary>
    public RequiredStartupAttribute()
    {
    }

    /// <summary>
    /// Gets a value indicating whether the specified type must be composed for every tenant.
    /// </summary>
    /// <param name="type">The type to inspect.</param>
    public static bool IsRequiredForType(Type type)
    {
        return type.GetCustomAttributes<RequiredStartupAttribute>(false).Any();
    }
}
