using System;
using Newtonsoft.Json.Linq;

namespace Orchard.Entities
{
    public static class EntityExtensions
    {
        /// <summary>
        /// Extracts the specified type of property.
        /// </summary>
        /// <typeparam name="T">The type of the property to extract.</typeparam>
        /// <returns>The default value of the requested type if the property was not found.</returns>
        public static T As<T>(this IEntity entity)
        {
            var typeName = typeof(T).Name;
            return entity.As<T>(typeName);
        }

        /// <summary>
        /// Extracts the specified named property.
        /// </summary>
        /// <typeparam name="T">The type of the property to extract.</typeparam>
        /// <param name="name">The name of the property to extract.</param>
        /// <returns>The default value of the requested type if the property was not found.</returns>
        public static T As<T>(this IEntity entity, string name)
        {
            JToken value;

            if (entity.Properties.TryGetValue(name, out value))
            {
                return value.ToObject<T>();
            }

            return default(T);
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
        /// <typeparam name="name">The name of the aspect.</typeparam>
        /// <typeparam name="action">An action to apply on the aspect.</typeparam>
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
