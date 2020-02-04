using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Taxonomies.ViewModels;

namespace OrchardCore.Taxonomies.Drivers
{
    public static class TaxonomyDriverHelper
    {
        public static List<TermEntryViewModel> PopulateTermEntries(IEnumerable<ContentItem> terms, BuildDisplayContext context)
        {
            var termsEntries = new List<TermEntryViewModel>();
            if (terms == null)
            {
                return termsEntries;
            }

            foreach (var contentItem in terms)
            {
                var children = Array.Empty<ContentItem>();

                if (contentItem.Content.Terms is JArray termsArray)
                {
                    children = termsArray.ToObject<ContentItem[]>();
                }

                termsEntries.Add(new TermEntryViewModel
                {
                    Term = contentItem,
                    BuildDisplayContext = context,
                    Terms = PopulateTermEntries(children, context)
                });
            }

            return termsEntries;
        }

    }
}
