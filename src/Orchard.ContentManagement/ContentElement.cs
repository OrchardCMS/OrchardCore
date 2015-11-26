using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Orchard.ContentManagement
{
    /// <summary>
    /// Common traits of <see cref="ContentItem"/>, <see cref="ContentPart"/>
    /// and <see cref="ContentField"/>
    /// </summary>
    public abstract class ContentElement
    {
        [JsonIgnore]
        public dynamic Content { get { return Data; } }
        internal abstract JObject Data { get; set; }

        public bool Has(string propertyName)
        {
            JToken value;
            return Data.TryGetValue(propertyName, out value);
        }

        public T Get<T>(string propertyName) where T : IContent
        {
            JToken value;
            if (Data.TryGetValue(propertyName, out value))
            {
                var obj = value as JObject;
                if (value == null)
                {
                    return default(T);
                }

                var result = obj.ToObject<T>();
                var contentElement = result as ContentElement;
                contentElement.Data = obj;

                return result;
            }

            return default(T);
        }

        public void Apply<T>(T property) where T : IContent
        {
            var obj = Data[typeof(T).Name] as JObject;

            // If the field is new for the content item, add it.
            // Otherwise merge the properties into the current obj.
            if (obj == null)
            {
                Weld(property);
            }
            else
            {
                obj.Merge(JObject.FromObject(property));
            }
        }

        /// <summary>
        /// Adds or replaces an existing part.
        /// </summary>
        public void Weld(object property)
        {
            Weld(property.GetType().Name, property);
        }

        /// <summary>
        /// Adds or replaces an existing part by its name.
        /// </summary>
        public void Weld(string name, object field)
        {
            Data[name] = JObject.FromObject(field);
        }
    }
}