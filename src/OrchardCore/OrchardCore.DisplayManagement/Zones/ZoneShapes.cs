using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.DisplayManagement.Descriptors;
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
        public async Task<IHtmlContent> ContentZone(dynamic DisplayAsync, dynamic Shape)
        {
            var htmlContents = new List<IHtmlContent>();

            var shapes = ((IEnumerable<dynamic>)Shape);

            var tabbed = shapes.GroupBy(x => (string)x.Metadata.Tab ?? "").ToList();

            if (tabbed.Count > 1)
            {
                var tabIndex = 0;
                var tabId = Shape.ContentItem != null ? (string)Shape.ContentItem.ContentItemId : "";
                var tabContentBuilder = new TagBuilder("div");
                tabContentBuilder.AddCssClass("tab-content");
                foreach (var tab in tabbed)
                {
                    var tabName = String.IsNullOrWhiteSpace(tab.Key) ? "Content" : tab.Key;
                    var tabItemBuilder = new TagBuilder("div");
                    tabItemBuilder.Attributes["id"] = $"tab-{tabId}-{tabName}".HtmlClassify();
                    var tabItemClasses = tabIndex == 0 ? "tab-pane fade show active" : "tab-pane fade";
                    tabItemBuilder.AddCssClass(tabItemClasses);
                    foreach (var item in tab)
                    {
                        tabItemBuilder.InnerHtml.AppendHtml(await DisplayAsync(item));
                    }
                    tabContentBuilder.InnerHtml.AppendHtml(tabItemBuilder);
                    tabIndex++;
                }
                htmlContents.Add(tabContentBuilder);
            }
            else if (tabbed.Count > 0)
            {
                foreach (var item in tabbed[0])
                {
                    htmlContents.Add(await DisplayAsync(item));
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
                    continue;

                if (!tabs.Contains(tab))
                    tabs.Add(tab);
            }

            // If we have any tabs, make sure we have at least the Content tab and that it is the first one,
            // since that's where we will put anything else not part of a tab.
            if (tabs.Any())
            {
                tabs.Remove("Content");
                tabs.Insert(0, "Content");
            }

            return tabs;
        }
    }
}
