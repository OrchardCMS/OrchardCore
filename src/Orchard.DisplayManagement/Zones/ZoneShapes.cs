using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Utility;
using Microsoft.AspNet.Mvc.Rendering;
using Orchard.DisplayManagement.Shapes;

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
        public void Zone(dynamic Display, dynamic Shape, TextWriter Output)
        {
            TagBuilder zoneWrapper = Orchard.DisplayManagement.Shapes.Shape.GetTagBuilder(Shape, "div");
            foreach (var item in Shape)
            {
                zoneWrapper.InnerHtml.Append(Display(item));
            }

            Output.Write(zoneWrapper);
        }

        [Shape]
        public void ContentZone(dynamic Display, dynamic Shape, TextWriter Output)
        {
            var unordered = ((IEnumerable<dynamic>)Shape).ToArray();
            var tabbed = unordered.GroupBy(x => (string)x.Metadata.Tab);

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
                    Output.Write(tabBuilder);
                }
            }
            else
            {
                foreach (var item in unordered)
                {
                    Output.Write(Display(item));
                }
            }
        }

        [Shape]
        public void DocumentZone(dynamic Display, dynamic Shape, TextWriter Output)
        {
            foreach (var item in Shape)
                Output.Write(Display(item));
        }
    }
}