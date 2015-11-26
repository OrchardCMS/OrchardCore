using System;
using System.Collections.Generic;
using System.Linq;

namespace Orchard.ContentManagement
{
    public static class ContentExtensions
    {
        public static bool Is<T>(this IContent content)
        {
            return content == null ? false : content.ContentItem.Has(typeof(T).Name);
        }
        public static T As<T>(this IContent content) where T : IContent
        {
            return content == null ? default(T) : content.ContentItem.Get<T>();
        }

        public static bool Has<T>(this IContent content)
        {
            return content == null ? false : content.ContentItem.Has(typeof(T).Name);
        }
        public static T Get<T>(this IContent content) where T : IContent
        {
            return content == null ? default(T) : content.ContentItem.Get<T>(typeof(T).Name);
        }

        public static IEnumerable<T> AsPart<T>(this IEnumerable<ContentItem> items) where T : IContent
        {
            return items == null ? null : items.Where(item => item.Is<T>()).Select(item => item.As<T>());
        }



        public static ContentItem With<T>(this ContentItem contentItem, Action<T> action) where T : IContent
        {
            if (action != null)
            {
                var part = contentItem.As<T>();
                if (part == null)
                {
                    return contentItem;
                }

                action(part);
                contentItem.Apply(part);
            }

            return contentItem;
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