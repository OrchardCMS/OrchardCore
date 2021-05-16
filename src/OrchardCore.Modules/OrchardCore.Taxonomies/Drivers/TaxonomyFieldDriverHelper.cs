using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.Taxonomies.Fields;
using OrchardCore.Taxonomies.ViewModels;

namespace OrchardCore.Taxonomies.Drivers
{
    public static class TaxonomyFieldDriverHelper
    {
        /// <summary>
        /// Populates a list of <see cref="TermEntry"/> with the hierarchy of terms.
        /// The list is ordered so that roots appear right before their child terms.
        /// </summary>
        public static void PopulateTermEntries(List<TermEntry> termEntries, TaxonomyField field, IEnumerable<ContentItem> contentItems, int level)
        {
            foreach (var contentItem in contentItems)
            {
                var children = Array.Empty<ContentItem>();

                if (contentItem.Content.Terms is JArray termsArray)
                {
                    children = termsArray.ToObject<ContentItem[]>();
                }

                var termEntry = new TermEntry
                {
                    Term = contentItem,
                    ContentItemId = contentItem.ContentItemId,
                    Selected = field.TermContentItemIds.Contains(contentItem.ContentItemId),
                    Level = level,
                    IsLeaf = children.Length == 0
                };

                termEntries.Add(termEntry);

                if (children.Length > 0)
                {
                    PopulateTermEntries(termEntries, field, children, level + 1);
                }
            }
        }
    }
}
