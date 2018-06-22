using System;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata.Builders;

namespace OrchardCore.ContentManagement
{
    public static class ContentExtensions
    {
        /// <summary>
        /// These settings instruct merge to replace current value, even for null values.
        /// </summary>
        private static readonly JsonMergeSettings JsonMergeSettings = new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Replace, MergeNullValueHandling = MergeNullValueHandling.Merge };
        
        /// <summary>
        /// Gets a content element by its name.
        /// </summary>
        /// <typeparam name="TElement">The expected type of the content element.</typeparam>
        /// <typeparam name="name">The name of the content element.</typeparam>
        /// <returns>The content element instance or <code>null</code> if it doesn't exist.</returns>
        public static TElement Get<TElement>(this ContentElement contentElement, string name) where TElement : ContentElement
        {
            return (TElement)contentElement.Get(typeof(TElement), name);
        }

        /// <summary>
        /// Gets a content element by its name.
        /// </summary>
        /// <typeparam name="contentElementType">The expected type of the content element.</typeparam>
        /// <typeparam name="name">The name of the content element.</typeparam>
        /// <returns>The content element instance or <code>null</code> if it doesn't exist.</returns>
        public static ContentElement Get(this ContentElement contentElement, Type contentElementType, string name)
        {
            var elementData = contentElement.Data[name] as JObject;

            if (elementData == null)
            {
                return null;
            }

            var result = (ContentElement)elementData.ToObject(contentElementType);
            result.Data = elementData;
            result.ContentItem = contentElement.ContentItem;

            return result;
        }

        /// <summary>
        /// Gets a content element by its name or create a new one.
        /// </summary>
        /// <typeparam name="TElement">The expected type of the content element.</typeparam>
        /// <typeparam name="name">The name of the content element.</typeparam>
        /// <returns>The content element instance or a new one if it doesn't exist.</returns>
        public static TElement GetOrCreate<TElement>(this ContentElement contentElement, string name) where TElement : ContentElement, new()
        {
            var existing = contentElement.Get<TElement>(name);

            if (existing == null)
            {
                existing = new TElement();
                existing.ContentItem = contentElement.ContentItem;
                contentElement.Data[name] = existing.Data;
                return existing;
            }

            return existing;
        }

        /// <summary>
        /// Adds a content element by name.
        /// </summary>
        /// <typeparam name="name">The name of the content element.</typeparam>
        /// <typeparam name="element">The element to add to the <see cref="ContentItem"/>.</typeparam>
        /// <returns>The current <see cref="ContentItem"/> instance.</returns>
        public static ContentElement Weld(this ContentElement contentElement, string name, ContentElement element)
        {
            JToken result;
            if (!contentElement.Data.TryGetValue(name, out result))
            {
                element.Data = JObject.FromObject(element, ContentBuilderSettings.IgnoreDefaultValuesSerializer);
                element.ContentItem = contentElement.ContentItem;

                contentElement.Data[name] = element.Data;
            }

            return contentElement;
        }

        /// <summary>
        /// Updates the content element with the specified name.
        /// </summary>
        /// <typeparam name="name">The name of the element to update.</typeparam>
        /// <typeparam name="element">The content element instance to update.</typeparam>
        /// <returns>The current <see cref="ContentItem"/> instance.</returns>
        public static ContentElement Apply(this ContentElement contentElement, string name, ContentElement element)
        {
            var elementData = contentElement.Data[name] as JObject;

            if (elementData != null)
            {
                elementData.Merge(JObject.FromObject(element), JsonMergeSettings);
            }
            else
            {
                contentElement.Data[name] = JObject.FromObject(element, ContentBuilderSettings.IgnoreDefaultValuesSerializer);
            }

            return contentElement;
        }

        /// <summary>
        /// Modifies a new or existing content element by name.
        /// </summary>
        /// <typeparam name="name">The name of the content element to update.</typeparam>
        /// <typeparam name="action">An action to apply on the content element.</typeparam>
        /// <returns>The current <see cref="ContentElement"/> instance.</returns>
        public static ContentElement Alter<TElement>(this ContentElement contentElement, string name, Action<TElement> action) where TElement : ContentElement, new()
        {
            var element = contentElement.GetOrCreate<TElement>(name);
            action(element);
            contentElement.Apply(name, element);

            return contentElement;
        }

        /// <summary>
        /// Updates the content item data.
        /// </summary>
        /// <returns>The current <see cref="ContentPart"/> instance.</returns>
        public static ContentPart Apply(this ContentPart contentPart)
        {
            contentPart.ContentItem.Apply(contentPart.GetType().Name, contentPart);
            return contentPart;
        }

        /// <summary>
        /// Whether the content element is published or not.
        /// </summary>
        /// <param name="content">The content to check.</param>
        /// <returns><c>True</c> if the content is published, <c>False</c> otherwise.</returns>
        public static bool IsPublished(this IContent content)
        {
            return content.ContentItem != null && content.ContentItem.Published;
        }

        /// <summary>
        /// Whether the content element has a draft or not.
        /// </summary>
        /// <param name="content">The content to check.</param>
        /// <returns><c>True</c> if the content has a draft, <c>False</c> otherwise.</returns>
        public static bool HasDraft(this IContent content)
        {
            return content.ContentItem != null && (!content.ContentItem.Published || !content.ContentItem.Latest);
        }
    }
}
