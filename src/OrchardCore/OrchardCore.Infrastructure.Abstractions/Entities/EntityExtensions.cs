using System.Text.Json;
using System.Text.Json.Nodes;

namespace OrchardCore.Entities;

public static class EntityExtensions
{
    /// <summary>
    /// Extracts the specified type of property.
    /// </summary>
    /// <param name="entity">The <see cref="IEntity"/>.</param>
    /// <typeparam name="T">The type of the property to extract.</typeparam>
    /// <returns>A new instance of the requested type if the property was not found.</returns>
    public static T As<T>(this IEntity entity, JsonSerializerOptions options = null)
        where T : new()
        => entity.As<T>(typeof(T).Name, options);

    /// <summary>
    /// Extracts the specified named property.
    /// </summary>
    /// <typeparam name="T">The type of the property to extract.</typeparam>
    /// <param name="entity">The <see cref="IEntity"/>.</param>
    /// <param name="name">The name of the property to extract.</param>
    /// <returns>A new instance of the requested type if the property was not found.</returns>
    public static T As<T>(this IEntity entity, string name, JsonSerializerOptions options = null)
        where T : new()
    {
        ArgumentNullException.ThrowIfNull(name);

        if (entity.Properties.TryGetPropertyValue(name, out var value))
        {
            return value.Deserialize<T>(options ?? JOptions.Default);
        }

        return new T();
    }

    /// <summary>
    /// Indicates if the specified type of property is attached to the <see cref="IEntity"/> instance.
    /// </summary>
    /// <typeparam name="T">The type of the property to check.</typeparam>
    /// <param name="entity">The <see cref="IEntity"/>.</param>
    /// <returns>True if the property was found, otherwise false.</returns>
    public static bool Has<T>(this IEntity entity)
        => entity.Has(typeof(T).Name);

    /// <summary>
    /// Indicates if the specified property is attached to the <see cref="IEntity"/> instance.
    /// </summary>
    /// <param name="entity">The <see cref="IEntity"/>.</param>
    /// <param name="name">The name of the property to check.</param>
    /// <returns>True if the property was found, otherwise false.</returns>
    public static bool Has(this IEntity entity, string name)
        => name != null && entity.Properties.ContainsKey(name);

    public static IEntity Put<T>(this IEntity entity, T aspect, JsonSerializerOptions options = null)
        where T : new()
        => entity.Put(typeof(T).Name, aspect, options);

    public static bool TryGet<T>(this IEntity entity, out T aspect, JsonSerializerOptions options = null)
        where T : new()
        => entity.TryGet(typeof(T).Name, out aspect, options);

    public static bool TryGet<T>(this IEntity entity, string name, out T aspect, JsonSerializerOptions options = null)
        where T : new()
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        if (entity.Properties.TryGetPropertyValue(name, out var value))
        {
            aspect = value.ToObject<T>(options);

            return true;
        }

        aspect = default;

        return false;
    }

    public static IEntity Put(this IEntity entity, string name, object value, JsonSerializerOptions options = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        entity.Properties[name] = JObject.FromObject(value, options);

        return entity;
    }

    /// <summary>
    /// Modifies or create an aspect.
    /// </summary>
    /// <param name="entity">The <see cref="IEntity"/>.</param>
    /// <param name="action">An action to apply on the aspect.</param>
    /// <returns>The current <see cref="IEntity"/> instance.</returns>
    public static IEntity Alter<TAspect>(this IEntity entity, Action<TAspect> action, JsonSerializerOptions options = null)
        where TAspect : new()
        => entity.Alter(typeof(TAspect).Name, action, options);

    /// <summary>
    /// Modifies or create an aspect.
    /// </summary>
    /// <param name="entity">The <see cref="IEntity"/>.</param>
    /// <param name="name">The name of the aspect.</param>
    /// <param name="action">An action to apply on the aspect.</param>
    /// <returns>The current <see cref="IEntity"/> instance.</returns>
    public static IEntity Alter<TAspect>(this IEntity entity, string name, Action<TAspect> action, JsonSerializerOptions options = null)
        where TAspect : new()
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        TAspect obj;

        if (!entity.Properties.TryGetPropertyValue(name, out var value))
        {
            obj = new TAspect();
        }
        else
        {
            obj = value.Deserialize<TAspect>(options ?? JOptions.Default);
        }

        action?.Invoke(obj);

        entity.Put(name, obj);

        return entity;
    }
}
