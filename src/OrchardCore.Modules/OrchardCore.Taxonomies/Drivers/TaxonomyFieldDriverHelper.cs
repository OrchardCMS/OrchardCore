using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
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
        public static void PopulateTermEntries(List<TermEntry> termEntries, TaxonomyField field, IEnumerable<ContentItem> contentItems, int level, JsonSerializerOptions jsonSerializerOptions = null)
        {
            foreach (var contentItem in contentItems)
            {
                var children = Array.Empty<ContentItem>();

                if (((JsonObject)contentItem.Content)["Terms"] is JsonArray termsArray)
                {
                    children = termsArray.ToObject<ContentItem[]>(jsonSerializerOptions);
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
                    PopulateTermEntries(termEntries, field, children, level + 1, jsonSerializerOptions);
                }
            }
        }
    }
}
