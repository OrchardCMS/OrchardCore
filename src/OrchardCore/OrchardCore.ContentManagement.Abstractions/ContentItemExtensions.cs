using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace OrchardCore.ContentManagement
{
    public static class ContentItemExtensions
    {
        /// <summary>
        /// Gets a content part by its type.
        /// </summary>
        /// <param name="contentItem">The <see cref="ContentItem"/>.</param>
        /// <typeparam name="TPart">The type of the content part.</typeparam>
        /// <returns>The content part or <code>null</code> if it doesn't exist.</returns>
        public static TPart As<TPart>(this ContentItem contentItem) where TPart : ContentPart
        {
            return contentItem.Get<TPart>(typeof(TPart).Name);
        }

        /// <summary>
        /// Gets a content part by its type or create a new one.
        /// </summary>
        /// <param name="contentItem">The <see cref="ContentItem"/>.</param>
        /// <typeparam name="TPart">The type of the content part.</typeparam>
        /// <returns>The content part instance or a new one if it doesn't exist.</returns>
        public static TPart GetOrCreate<TPart>(this ContentItem contentItem) where TPart : ContentPart, new()
        {
            return contentItem.GetOrCreate<TPart>(typeof(TPart).Name);
        }

        /// <summary>
        /// Removes a content part by its type.
        /// </summary>
        /// <param name="contentItem">The <see cref="ContentItem"/>.</param>
        /// <typeparam name="TPart">The type of the content part.</typeparam>
        public static void Remove<TPart>(this ContentItem contentItem) where TPart : ContentPart, new()
        {
            contentItem.Remove(typeof(TPart).Name);
        }

        /// <summary>
        /// Adds a content part by its type.
        /// </summary>
        /// <param name="contentItem">The <see cref="ContentItem"/>.</param>
        /// <param name="part">The content part to weld.</param>
        /// <typeparam name="TPart">The type of the content part.</typeparam>
        /// <returns>The current <see cref="ContentItem"/> instance.</returns>
        public static ContentItem Weld<TPart>(this ContentItem contentItem, TPart part) where TPart : ContentPart
        {
            contentItem.Weld(typeof(TPart).Name, part);
            return contentItem;
        }

        /// <summary>
        /// Updates the content part with the specified type.
        /// </summary>
        /// <param name="contentItem">The <see cref="ContentItem"/>.</param>
        /// <param name="part">The content part to update.</param>
        /// <typeparam name="TPart">The type of the content part.</typeparam>
        /// <returns>The current <see cref="ContentItem"/> instance.</returns>
        public static ContentItem Apply<TPart>(this ContentItem contentItem, TPart part) where TPart : ContentPart
        {
            contentItem.Apply(typeof(TPart).Name, part);
            return contentItem;
        }

        /// <summary>
        /// Modifies a new or existing content part by name.
        /// </summary>
        /// <param name="contentItem">The <see cref="ContentItem"/>.</param>
        /// <param name="action">An action to apply on the content part.</param>
        /// <typeparam name="TPart">The type of the content part.</typeparam>
        /// <returns>The current <see cref="ContentPart"/> instance.</returns>
        public static ContentItem Alter<TPart>(this ContentItem contentItem, Action<TPart> action) where TPart : ContentPart, new()
        {
            var part = contentItem.GetOrCreate<TPart>();
            action(part);
            contentItem.Apply(part);

            return contentItem;
        }

        /// <summary>
        /// Modifies a new or existing content part by name.
        /// </summary>
        /// <param name="contentItem">The <see cref="ContentItem"/>.</param>
        /// <param name="action">An action to apply on the content part.</param>
        /// <typeparam name="TPart">The type of the content part.</typeparam>
        /// <returns>The current <see cref="ContentItem"/> instance.</returns>
        public static async Task<ContentItem> AlterAsync<TPart>(this ContentItem contentItem, Func<TPart, Task> action) where TPart : ContentPart, new()
        {
            var part = contentItem.GetOrCreate<TPart>();
            await action(part);
            contentItem.Apply(part);

            return contentItem;
        }

        /// <summary>
        /// Merges properties to the contents of a content item.
        /// </summary>
        /// <param name="contentItem">The <see cref="ContentItem"/>.</param>
        /// <param name="properties">The object to merge.</param>
        /// <param name="jsonMergeSettings">The <see cref="JsonMergeSettings"/> to use.</param>
        /// <returns>The current <see cref="ContentItem"/> instance.</returns>
        public static ContentItem Merge(this ContentItem contentItem, object properties, JsonMergeSettings jsonMergeSettings = null)
        {
            var props = JObject.FromObject(properties);

            var originalDocumentId = contentItem.Id;
            contentItem.Data.Merge(props, jsonMergeSettings);
            contentItem.Elements.Clear();

            // Return to original value or it will be interpreated as a different object by YesSql.
            contentItem.Id = originalDocumentId;

            // After merging content here we need to remove all the well known properties from the Data jObject
            // or these properties will take precedence over the properties on the C# object when and if they are mutated.
            if (props.ContainsKey(nameof(contentItem.DisplayText)))
            {
                contentItem.DisplayText = props[nameof(contentItem.DisplayText)].ToString();
                contentItem.Data.Remove(nameof(contentItem.DisplayText));
            }

            if (props.ContainsKey(nameof(contentItem.Owner)))
            {
                contentItem.Owner = props[nameof(contentItem.Owner)].ToString();
                contentItem.Data.Remove(nameof(contentItem.Owner));
            }

            if (props.ContainsKey(nameof(contentItem.Author)))
            {
                contentItem.Author = props[nameof(contentItem.Author)].ToString();
                contentItem.Data.Remove(nameof(contentItem.Author));
            }

            // Do not set these properties on the content item as they are the responsibility of the content manager.
            if (props.ContainsKey(nameof(contentItem.Published)))
            {
                contentItem.Data.Remove(nameof(contentItem.Published));
            }

            if (props.ContainsKey(nameof(contentItem.Latest)))
            {
                contentItem.Data.Remove(nameof(contentItem.Latest));
            }

            if (props.ContainsKey(nameof(contentItem.CreatedUtc)))
            {
                contentItem.Data.Remove(nameof(contentItem.CreatedUtc));
            }

            if (props.ContainsKey(nameof(contentItem.ModifiedUtc)))
            {
                contentItem.Data.Remove(nameof(contentItem.ModifiedUtc));
            }

            if (props.ContainsKey(nameof(contentItem.PublishedUtc)))
            {
                contentItem.Data.Remove(nameof(contentItem.PublishedUtc));
            }

            if (props.ContainsKey(nameof(contentItem.ContentItemId)))
            {
                contentItem.Data.Remove(nameof(contentItem.ContentItemId));
            }

            if (props.ContainsKey(nameof(contentItem.ContentItemVersionId)))
            {
                contentItem.Data.Remove(nameof(contentItem.ContentItemVersionId));
            }

            if (props.ContainsKey(nameof(contentItem.ContentType)))
            {
                contentItem.Data.Remove(nameof(contentItem.ContentType));
            }

            return contentItem;
        }
    }
}
