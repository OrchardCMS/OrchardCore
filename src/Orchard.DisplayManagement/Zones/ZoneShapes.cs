using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Utility;
using Microsoft.AspNet.Mvc.Rendering;
using Orchard.DisplayManagement.Shapes;
using Microsoft.AspNet.Html.Abstractions;
using Orchard.DisplayManagement.Implementation;
using System.Threading.Tasks;

namespace Orchard.DisplayManagement.Zones
{
    public class ZoneShapes : IShapeTableProvider
    {
        public void Discover(ShapeTableBuilder builder)
        {

            // 'Zone' shapes are built on the Zone base class
            // They have class="zone zone-{name}"
            // and the template can be specialized with "Zone-{Name}" base file name
            builder.Describe("Zone")
                .OnDisplaying(displaying =>
                {
                    var zone = displaying.Shape;
                    string zoneName = zone.ZoneName;
                    zone.Classes.Add("zone-" + zoneName.HtmlClassify());
                    zone.Classes.Add("zone");

                    // Zone__[ZoneName] e.g. Zone-SideBar
                    zone.Metadata.Alternates.Add("Zone__" + zoneName);
                });
        }

        [Shape]
        public IHtmlContent Zone(dynamic Display, dynamic Shape)
        {
            TagBuilder zoneWrapper = Orchard.DisplayManagement.Shapes.Shape.GetTagBuilder(Shape, "div");
            foreach (var item in Shape)
            {
                zoneWrapper.InnerHtml.Append(Display(item));
            }

            return zoneWrapper;
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