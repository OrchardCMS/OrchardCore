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
        private Dictionary<ContentItem, int> _recursions = new Dictionary<ContentItem, int>();

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
