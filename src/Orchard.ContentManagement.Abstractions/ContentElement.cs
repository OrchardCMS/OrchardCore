using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Orchard.ContentManagement
{
    /// <summary>
    /// Common traits of <see cref="ContentItem"/>, <see cref="ContentPart"/>
    /// and <see cref="ContentField"/>
    /// </summary>
    public class ContentElement : IContent
    {
        protected ContentElement() : this(new JObject())
        {
        }

        protected ContentElement(JObject data)
        {
            Data = data;
        }

        [JsonIgnore]
        public dynamic Content { get { return Data; } }

        [JsonIgnore]
        internal JObject Data { get; set; }

        [JsonIgnore]
        public ContentItem ContentItem { get; protected set; }

        /// <summary>
        /// Whether the content has a named property or not.
        /// </summary>
        /// <param name="name">The name of the property to look for.</param>
        public bool Has(string name)
        {
            JToken value;
            return Data.TryGetValue(name, out value);
        }

        /// <summary>
        /// Projects the content to a custom type.
        /// </summary>
        public T Get<T>(string name) where T : ContentElement
        {
            JToken value;
            if (Data.TryGetValue(name, out value))
            {
                var obj = value as JObject;
                if (obj == null)
                {
                    return default(T);
                }

                var result = obj.ToObject<T>();
                result.Data = obj;
                result.ContentItem = ((IContent)this).ContentItem;

                return result;
            }

            return default(T);
        }

        public T GetOrCreate<T>(string name) where T : ContentElement, new()
        {
            JToken value;
            T contentElement = default(T);

            if (Data.TryGetValue(name, out value))
            {
                var obj = value as JObject;
                if (obj == null)
                {
                    return default(T);
                }

                contentElement = obj.ToObject<T>();
                contentElement.Data = obj;
            }
            else
            {
                contentElement = new T();
                contentElement.Data = new JObject();
                Data[name] = contentElement.Data;
            }

            contentElement.ContentItem = ((IContent)this).ContentItem;

            return contentElement;
        }

        public ContentElement Get(Type t, string name)
        {
            JToken value;
            if (Data.TryGetValue(name, out value))
            {
                var obj = value as JObject;
                if (obj == null)
                {
                    return null;
                }

                var contentElement = obj.ToObject(t) as ContentElement;
                contentElement.Data = obj;
                contentElement.ContentItem = ((IContent)this).ContentItem;

                return contentElement;
            }

            return null;
        }

        public ContentElement GetOrCreate(Type t, Func<ContentElement> factory, string name)
        {
            JToken value;
            ContentElement contentElement;

            if (Data.TryGetValue(name, out value))
            {
                var obj = value as JObject;
                if (obj == null)
                {
                    return null;
                }

                contentElement = obj.ToObject(t) as ContentElement;
                contentElement.Data = obj;
            }
            else
            {
                contentElement = factory();
                contentElement.Data = new JObject();
                Data[name] = contentElement.Data;
            }

            contentElement.ContentItem = ((IContent)this).ContentItem;
            return contentElement;
        }

        /// <summary>
        /// Extract some named content.
        /// </summary>
        public ContentElement Get(string name)
        {
            JToken value;
            if (Data.TryGetValue(name, out value))
            {
                var obj = value as JObject;
                if (obj == null)
                {
                    return null;
                }

                var contentElement = new ContentElement(obj);
                contentElement.ContentItem = ((IContent)this).ContentItem;

                return contentElement;
            }

            return null;
        }

        /// <summary>
        /// Adds or replaces a projection by its name.
        /// </summary>
        public void Weld(string name, object obj)
        {
            Data[name] = JObject.FromObject(obj);
        }
    }
}