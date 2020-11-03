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
        bool IsRecursive(ContentItem contentItem, int maxRecursions = 1);
    }

    /// <inheritdocs />
    public class ContentItemRecursionHelper<T> : IContentItemRecursionHelper<T> where T : ILiquidFilter
    {
        private HashSet<string> _contentItemIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, int> _recursions = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        /// <inheritdocs />
        public bool IsRecursive(ContentItem contentItem, int maxRecursions = 1)
        {
            if (_contentItemIds.Contains(contentItem.ContentItemId))
            {
                if (maxRecursions > 1)
                {
                    if (_recursions.TryGetValue(contentItem.ContentItemId, out var counter))
                    {
                        if (counter == maxRecursions)
                        {
                            return true;
                        }

                        _recursions[contentItem.ContentItemId] = counter + 1;
                        return false;
                    }
                    else
                    {
                        _recursions[contentItem.ContentItemId] = 1;
                        return false;
                    }
                }

                return true;
            }

            _contentItemIds.Add(contentItem.ContentItemId);

            return false;
        }
    }
}
