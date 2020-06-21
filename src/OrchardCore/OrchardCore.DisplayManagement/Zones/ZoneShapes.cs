using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore.DisplayManagement.Zones
{
    public class ZoneShapes : IShapeAttributeProvider
    {
        [Shape]
        public async Task<IHtmlContent> Zone(dynamic DisplayAsync, dynamic Shape)
        {
            var htmlContentBuilder = new HtmlContentBuilder();
            foreach (var item in Shape)
            {
                htmlContentBuilder.AppendHtml(await DisplayAsync(item));
            }

            return htmlContentBuilder;
        }

        [Shape]
        public async Task<IHtmlContent> ContentZone(dynamic DisplayAsync, dynamic Shape, dynamic New, DisplayContext DisplayContext)
        {
            var htmlContents = new List<IHtmlContent>();

            var shapes = ((IEnumerable<dynamic>)Shape);

            var tabs = shapes.GroupBy(x => (string)x.Metadata.Tab ?? "").ToList();

            // Process tabs first, then Cards, then Columns.
            if (tabs.Count > 1)
            {
                dynamic tabContainer = await New.TabContainer(ContentItem: Shape.ContentItem, Tabs: tabs);

                htmlContents.Add(await DisplayAsync(tabContainer));
            }
            else// if (tabs.Count > 0)
            {
                var cards = tabs[0].GroupBy(x => (string)x.Metadata.Card ?? "").ToList();
                if (cards.Count > 1)
                {
                    dynamic cardContainer = await New.CardContainer(ContentItem: Shape.ContentItem, Cards: cards);

                    htmlContents.Add(await DisplayAsync(cardContainer));
                }
                else// if (cards.Count > 0)
                {
                    // any unspecified columns will be grouped into Content
                    var columns = tabs[0].GroupBy(x =>
                    {
                        // By convention all placement delimiters default to the name 'Content' when not specified during placement.
                        var key = (string)x.Metadata.Column ?? "Content";

                        var modifierIndex = key.IndexOf('-');
                        if (modifierIndex != -1)
                        {
                            key = key.Substring(0, modifierIndex);
                        }

                        return key;
                    }).ToList();

                    if (columns.Count > 1)
                    {
                        dynamic columnContainer = await New.ColumnContainer(ContentItem: Shape.ContentItem, Columns: columns);
                        htmlContents.Add(await DisplayAsync(columnContainer));
                    }
                    else
                    {
                        foreach (var item in tabs[0])
                        {
                            htmlContents.Add(await DisplayAsync(item));
                        }
                    }
                }
            }

            var htmlContentBuilder = new HtmlContentBuilder();
            foreach (var htmlContent in htmlContents)
            {
                htmlContentBuilder.AppendHtml(htmlContent);
            }

            return htmlContentBuilder;
        }

        public static IEnumerable<string> HarvestAndSortTabs(IEnumerable<dynamic> shapes)
        {
            var tabs = new List<string>();

            foreach (var shape in shapes)
            {
                var tab = (string)shape.Metadata.Tab;

                if (String.IsNullOrEmpty(tab))
                {
                    continue;
                }

                if (!tabs.Contains(tab))
                {
                    tabs.Add(tab);
                }
            }

            // If we have any tabs, make sure we have at least the Content tab and that it is the first one,
            // since that's where we will put anything else not part of a tab.
            if (tabs.Any())
            {
                // TODO change this so we don't get a Content tab if there is nothing to go in it.
                tabs.Remove("Content");
                tabs.Insert(0, "Content");
            }

            return tabs;
        }
    }
}
