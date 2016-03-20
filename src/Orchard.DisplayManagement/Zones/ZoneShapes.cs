using System;
using System.Collections.Generic;
using System.Linq;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Utility;
using Microsoft.AspNet.Mvc.Rendering;
using Orchard.DisplayManagement.Shapes;
using Microsoft.AspNet.Html;
using Orchard.DisplayManagement.Implementation;

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

            return new DisplayHelper.Combined(htmlContents);
        }

        [Shape]
        public IHtmlContent ContentZone(dynamic Display, dynamic Shape)
        {
            var htmlContents = new List<IHtmlContent>();

            var shapes = ((IEnumerable<dynamic>)Shape);
            var tabbed = shapes.GroupBy(x => (string)x.Metadata.Tab);

            if (tabbed.Count() > 1)
            {
                foreach (var tab in tabbed)
                {
                    var tabName = String.IsNullOrWhiteSpace(tab.Key) ? "Content" : tab.Key;
                    var tabBuilder = new TagBuilder("div");
                    tabBuilder.Attributes["id"] = "tab-" + tabName.HtmlClassify();
                    tabBuilder.Attributes["data-tab"] = tabName;
                    foreach (var item in tab)
                    {
                        tabBuilder.InnerHtml.Append(Display(item));
                    }
                    htmlContents.Add(tabBuilder);
                }
            }
            else
            {
                foreach (var item in tabbed.First())
                {
                    htmlContents.Add(Display(item));
                }
            }

            // TODO: Replace by HtmlContentBuilder when available
            var combined = new DisplayHelper.Combined(htmlContents);

            return combined;
        }
    }
}