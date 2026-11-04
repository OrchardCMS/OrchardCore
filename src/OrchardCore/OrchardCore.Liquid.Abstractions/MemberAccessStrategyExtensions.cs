using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Fluid;
using Fluid.Accessors;

namespace OrchardCore.Liquid;

public static class MemberAccessStrategyExtensions
{
    private const DynamicallyAccessedMemberTypes RegisteredMemberTypes = DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicFields;

    // The accessors of a type are shared by all the tenants, as they are costly to create.
    private static readonly ConcurrentDictionary<Type, KeyValuePair<string, MemberAccessor>[]> s_typeMembers = new();

    /// <summary>
    /// Registers a type and all its public properties and fields.
    /// </summary>
    /// <typeparam name="T">The type to register.</typeparam>
    /// <param name="strategy">The <see cref="MemberAccessStrategy"/>.</param>
    public static void Register<[DynamicallyAccessedMembers(RegisteredMemberTypes)] T>(this MemberAccessStrategy strategy)
        => strategy.Register(typeof(T));

    /// <summary>
    /// Registers a type and all its public properties and fields.
    /// </summary>
    /// <param name="strategy">The <see cref="MemberAccessStrategy"/>.</param>
    /// <param name="type">The type to register.</param>
    public static void Register(this MemberAccessStrategy strategy, [DynamicallyAccessedMembers(RegisteredMemberTypes)] Type type)
    {
        ArgumentNullException.ThrowIfNull(strategy);
        ArgumentNullException.ThrowIfNull(type);

        foreach (var member in s_typeMembers.GetOrAdd(type, GetTypeMembers))
        {
            strategy.Register(type, member.Key, member.Value);
        }
    }

    private static KeyValuePair<string, MemberAccessor>[] GetTypeMembers([DynamicallyAccessedMembers(RegisteredMemberTypes)] Type type)
    {
        var members = new Dictionary<string, MemberAccessor>();

        foreach (var propertyInfo in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (propertyInfo.GetIndexParameters().Length > 0 || propertyInfo.GetGetMethod() == null)
            {
                continue;
            }

            members[propertyInfo.Name] = IsTask(propertyInfo.PropertyType)
                ? new AsyncDelegateAccessor((obj, _) => GetTaskResultAsync((Task)propertyInfo.GetValue(obj)))
                : new PropertyInfoAccessor(propertyInfo);
        }

        foreach (var fieldInfo in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            members[fieldInfo.Name] = IsTask(fieldInfo.FieldType)
                ? new AsyncDelegateAccessor((obj, _) => GetTaskResultAsync((Task)fieldInfo.GetValue(obj)))
                : new FieldInfoAccessor(fieldInfo);
        }

        return [.. members];
    }

    private static bool IsTask(Type type)
        => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>);

    private static async Task<object> GetTaskResultAsync(Task task)
    {
        await task;

        return ((dynamic)task).Result;
    }
}
