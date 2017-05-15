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
    }
}
