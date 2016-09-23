using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Shapes;
using Orchard.Utility;

namespace Orchard.DisplayManagement.Zones
{
    public class ZoneShapes : IShapeTableProvider
    {
        public void Discover(ShapeTableBuilder builder)
        {
        }

        [Shape]
        public IHtmlContent Zone(dynamic Display, dynamic Shape)
        {
            var htmlContents = new List<IHtmlContent>();
            foreach (var item in Shape)
            {
                htmlContents.Add(Display(item));
            }

            var htmlContentBuilder = new HtmlContentBuilder();
            foreach (var htmlContent in htmlContents)
            {
                htmlContentBuilder.AppendHtml(htmlContent);
            }

            return htmlContentBuilder;
        }

        [Shape]
        public IHtmlContent ContentZone(dynamic Display, dynamic Shape)
        {
            var htmlContents = new List<IHtmlContent>();

            var shapes = ((IEnumerable<dynamic>)Shape);
            var tabbed = shapes.GroupBy(x => (string)x.Metadata.Tab).ToList();

            if (tabbed.Count > 1)
            {
                foreach (var tab in tabbed)
                {
                    var tabName = String.IsNullOrWhiteSpace(tab.Key) ? "Content" : tab.Key;
                    var tabBuilder = new TagBuilder("div");
                    tabBuilder.Attributes["id"] = "tab-" + tabName.HtmlClassify();
                    tabBuilder.Attributes["data-tab"] = tabName;
                    foreach (var item in tab)
                    {
                        tabBuilder.InnerHtml.AppendHtml(Display(item));
                    }
                    htmlContents.Add(tabBuilder);
                }
            }
            else if (tabbed.Count > 0)
            {
                foreach (var item in tabbed[0])
                {
                    htmlContents.Add(Display(item));
                }
            }

            var htmlContentBuilder = new HtmlContentBuilder();
            foreach (var htmlContent in htmlContents)
            {
                htmlContentBuilder.AppendHtml(htmlContent);
            }

            return htmlContentBuilder;
        }
    }
}