using System;
using Newtonsoft.Json.Linq;

namespace OrchardCore.Entities
{
    public static class EntityExtensions
    {
        /// <summary>
        /// Extracts the specified type of property.
        /// </summary>
        /// <param name="entity">The <see cref="IEntity"/>.</param>
        /// <typeparam name="T">The type of the property to extract.</typeparam>
        /// <returns>A new instance of the requested type if the property was not found.</returns>
        public static T As<T>(this IEntity entity) where T : new()
        {
            var typeName = typeof(T).Name;
            return entity.As<T>(typeName);
        }

        /// <summary>
        /// Extracts the specified named property.
        /// </summary>
        /// <typeparam name="T">The type of the property to extract.</typeparam>
        /// <param name="entity">The <see cref="IEntity"/>.</param>
        /// <param name="name">The name of the property to extract.</param>
        /// <returns>A new instance of the requested type if the property was not found.</returns>
        public static T As<T>(this IEntity entity, string name) where T : new()
        {
            JToken value;

            if (entity.Properties.TryGetValue(name, out value))
            {
                return value.ToObject<T>();
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
        {
            var typeName = typeof(T).Name;
            return entity.Has(typeName);
        }

        /// <summary>
        /// Indicates if the specified property is attached to the <see cref="IEntity"/> instance.
        /// </summary>
        /// <param name="entity">The <see cref="IEntity"/>.</param>
        /// <param name="name">The name of the property to check.</param>
        /// <returns>True if the property was found, otherwise false.</returns>
        public static bool Has(this IEntity entity, string name)
        {
            return entity.Properties[name] != null;
        }

        public static IEntity Put<T>(this IEntity entity, T aspect) where T : new()
        {
            return entity.Put(typeof(T).Name, aspect);
        }

        public static IEntity Put(this IEntity entity, string name, object property)
        {
            entity.Properties[name] = JObject.FromObject(property);
            return entity;
        }

        /// <summary>
        /// Modifies or create an aspect.
        /// </summary>
        /// <param name="entity">The <see cref="IEntity"/>.</param>
        /// <param name="name">The name of the aspect.</param>
        /// <param name="action">An action to apply on the aspect.</param>
        /// <returns>The current <see cref="IEntity"/> instance.</returns>
        public static IEntity Alter<TAspect>(this IEntity entity, string name, Action<TAspect> action) where TAspect : new()
        {
            JToken value;
            TAspect obj;

            if (!entity.Properties.TryGetValue(name, out value))
            {
                obj = new TAspect();
            }
            else
            {
                obj = value.ToObject<TAspect>();
            }

            action?.Invoke(obj);

            entity.Put(name, obj);

            return entity;
        }
    }
}
