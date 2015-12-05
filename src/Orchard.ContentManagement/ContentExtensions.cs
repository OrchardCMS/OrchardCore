using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.ContentManagement
{
    public static class ContentExtensions
    {
        public static bool Is<T>(this IContent content)
        {
            return content == null ? false : content.ContentItem.Has<T>();
        }

        public static T As<T>(this IContent content) where T : IContent
        {
            return content == null ? default(T) : content.ContentItem.Get<T>();
        }

        public static bool Has<T>(this IContent content)
        {
            return content == null ? false : content.ContentItem.Has(typeof(T).Name);
        }

        public static T Get<T>(this ContentElement content) where T : IContent
        {
            return content == null ? default(T) : content.Get<T>(typeof(T).Name);
        }

        public static ContentElement Weld<T>(this ContentElement content) where T : ContentElement, new()
        {
            return content.Weld<T>(typeof(T).Name);
        }

        public static ContentElement Weld<T>(this ContentElement content, string name) where T : ContentElement, new()
        {
            var t = new T();
            content.Weld(name, t);
            return t;
        }

        public static void Weld(this ContentElement content, ContentElement property)
        {
            content.Weld(property.GetType().Name, property);
        }

        public static IEnumerable<T> AsPart<T>(this IEnumerable<ContentItem> items) where T : IContent
        {
            return items == null ? null : items.Where(item => item.Is<T>()).Select(item => item.As<T>());
        }

        public static ContentItem Alter<T>(this ContentItem contentItem, Action<T> action) where T : ContentPart, new()
        {
            AlterContentElement<T>(contentItem, typeof(T).Name, action);
            return contentItem;
        }

        public static ContentPart Alter<T>(this ContentPart contentPart, string fieldName, Action<T> action) where T : ContentField
        {
            AlterContentElement<T>(contentPart, fieldName, action);
            return contentPart;
        }

        private static ContentElement AlterContentElement<T>(this ContentElement contentElement, string propertyName, Action<T> action) where T : ContentElement
        {
            if (action != null)
            {
                JToken value;
                T slice;
                
                // Extract the object from the current content to pass to the action
                if (contentElement.Data.TryGetValue(propertyName, out value))
                {
                    slice = value.ToObject<T>();
                }
                else
                {
                    throw new ArgumentException("Content doesn't have property named \"{0}\"", propertyName);
                }

                action(slice);
                
                // Get the properties from the slice
                var jObj = JObject.FromObject(slice);
                contentElement.Data[propertyName] = jObj;

                // Merge any properties that were added to slice in an inner call to Alter
                jObj.Merge(slice.Data);
            }

            return contentElement;
        }

        public static bool IsPublished(this IContent content)
        {
            return content.ContentItem != null && content.ContentItem.Published;
        }

        public static bool HasDraft(this IContent content)
        {
            return content.ContentItem != null &&
                   content.ContentItem.Published == false ||
                   (content.ContentItem.Published && content.ContentItem.Latest == false);
        }
    }
}