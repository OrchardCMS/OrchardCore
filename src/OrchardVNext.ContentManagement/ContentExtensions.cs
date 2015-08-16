using System.Collections.Generic;
using System.Linq;

namespace OrchardVNext.ContentManagement {
    public static class ContentExtensions {
        /* Aggregate item/part type casting extension methods */

        public static bool Is<T>(this IContent content) {
            return content == null ? false : content.ContentItem.Has(typeof(T));
        }
        public static T As<T>(this IContent content) where T : IContent {
            return content == null ? default(T) : (T)content.ContentItem.Get(typeof(T));
        }

        public static bool Has<T>(this IContent content) {
            return content == null ? false : content.ContentItem.Has(typeof(T));
        }
        public static T Get<T>(this IContent content) where T : IContent {
            return content == null ? default(T) : (T)content.ContentItem.Get(typeof(T));
        }

        public static IEnumerable<T> AsPart<T>(this IEnumerable<ContentItem> items) where T : IContent {
            return items == null ? null : items.Where(item => item.Is<T>()).Select(item => item.As<T>());
        }

        public static bool IsPublished(this IContent content) {
            return content.ContentItem.VersionRecord != null && content.ContentItem.VersionRecord.Published;
        }
        public static bool HasDraft(this IContent content) {
            return (
                       (content.ContentItem.VersionRecord != null)
                       && ((content.ContentItem.VersionRecord.Published == false)
                           || (content.ContentItem.VersionRecord.Published && content.ContentItem.VersionRecord.Latest == false)));
        }
    }
}
