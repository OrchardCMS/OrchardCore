using System;
using System.Collections.Generic;
using OrchardCore.ContentManagement;
using OrchardCore.Liquid;

namespace OrchardCore.Contents.Liquid
{
    /// <summary>
    /// Prevents a content item being called by an implementation of a <see cref="ILiquidFilter"/> recursivly.
    /// </summary>
    public interface IContentItemRecursionHelper<T> where T : ILiquidFilter
    {
        /// <summary>
        /// Returns <see langword="True"/> when the <see cref="ContentItem"/> has already been evaluated during this request by the particular filter./>
        /// </summary>
        bool IsRecursive(ContentItem contentItem);
    }

    /// <inheritdocs />
    public class ContentItemRecursionHelper<T> : IContentItemRecursionHelper<T> where T : ILiquidFilter
    {
        private HashSet<string> _contentItemIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <inheritdocs />
        public bool IsRecursive(ContentItem contentItem)
        {
            if (_contentItemIds.Contains(contentItem.ContentItemId))
            {
                return true;
            }

            _contentItemIds.Add(contentItem.ContentItemId);

            return false;
        }
    }
}
