using System.Collections.Generic;
using OrchardCore.ContentManagement;

namespace OrchardCore.Contents.Liquid
{
    public interface IContentItemRecursionHelper<T>
    {
        /// <summary>
        /// Returns <see langword="True"/> when the <see cref="ContentItem"/> has already been evaluated during this request by the particular filter./>
        /// </summary>
        bool IsRecursive(ContentItem contentItem, int maxRecursions = 1);
    }

    /// <inheritdocs />
    public class ContentItemRecursionHelper<T> : IContentItemRecursionHelper<T>
    {
        private readonly Dictionary<ContentItem, int> _recursions = new();

        /// <inheritdocs />
        public bool IsRecursive(ContentItem contentItem, int maxRecursions = 1)
        {
            if (_recursions.ContainsKey(contentItem))
            {
                var counter = _recursions[contentItem];
                if (maxRecursions < 1)
                {
                    maxRecursions = 1;
                }

                if (counter > maxRecursions)
                {
                    return true;
                }

                _recursions[contentItem] = counter + 1;
                return false;
            }

            _recursions[contentItem] = 1;
            return false;
        }
    }
}
