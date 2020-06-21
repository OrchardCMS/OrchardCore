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

            // Is this any use as a shape?
            var tabs = shapes.GroupBy(x =>
            {
                // By convention all placement delimiters default to the name 'Content' when not specified during placement.
                // so this is why it gets werid.
                var key = (string)x.Metadata.Tab;
                if (String.IsNullOrEmpty(key))
                {
                    key = "Content";
                }
                // var key = (string)x.Metadata.Tab ?? "Content";

                //TODO placement modifier
                // var modifierIndex = key.IndexOf('-');
                // if (modifierIndex != -1)
                // {
                //     key = key.Substring(0, modifierIndex);
                // }

                return key;
            }).ToList();

            // Process tabs first, then Cards, then Columns.
            if (tabs.Count > 1)
            {
                dynamic tabContainer = await New.TabContainer(ContentItem: Shape.ContentItem, Tabs: tabs);

                htmlContents.Add(await DisplayAsync(tabContainer));
            }
            else
            {
                // This determins whether to show a card, or fall back to columns
                var cardGrouping = await New.CardGrouping(Grouping: tabs[0]);

                htmlContents.Add(await DisplayAsync(cardGrouping));
            }

            var htmlContentBuilder = new HtmlContentBuilder();
            foreach (var htmlContent in htmlContents)
            {
                htmlContentBuilder.AppendHtml(htmlContent);
            }

            return htmlContentBuilder;
        }

        [Shape]
        public async Task<IHtmlContent> CardGrouping(dynamic DisplayAsync, dynamic Shape, dynamic New)
        {
            var htmlContentBuilder = new HtmlContentBuilder();
            IGrouping<string, dynamic> grouping = Shape.Grouping;

            var groupings = grouping.GroupBy(x =>
            {
                // By convention all placement delimiters default to the name 'Content' when not specified during placement.
                var key = (string)x.Metadata.Card ?? "Content";

                //TODO placement modifier
                // var modifierIndex = key.IndexOf('-');
                // if (modifierIndex != -1)
                // {
                //     key = key.Substring(0, modifierIndex);
                // }

                return key;
            }).ToList();

            if (groupings.Count > 1)
            {
                dynamic container = await New.CardContainer(ContentItem: Shape.ContentItem, Cards: groupings);
                htmlContentBuilder.AppendHtml(await DisplayAsync(container));
            }
            else
            {
                // See if there are columns.
                var groupingShape = await New.ColumnGrouping(Grouping: grouping);
                htmlContentBuilder.AppendHtml(await DisplayAsync(groupingShape));
            }

            return htmlContentBuilder;
        }

        [Shape]
        public async Task<IHtmlContent> ColumnGrouping(dynamic DisplayAsync, dynamic Shape, dynamic New)
        {
            var htmlContentBuilder = new HtmlContentBuilder();
            IGrouping<string, dynamic> grouping = Shape.Grouping;

            var groupings = grouping.GroupBy(x =>
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

            if (groupings.Count > 1)
            {
                dynamic container = await New.ColumnContainer(ContentItem: Shape.ContentItem, Columns: groupings);
                htmlContentBuilder.AppendHtml(await DisplayAsync(container));
            }
            else
            {
                foreach (var item in grouping)
                {
                    htmlContentBuilder.AppendHtml(await DisplayAsync(item));
                }
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
