using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orchard.DisplayManagement.Descriptors;
using Orchard.UI;
using Orchard.Utility;
using Microsoft.AspNet.Mvc.Rendering;
using Orchard.DisplayManagement.Shapes;

// ReSharper disable InconsistentNaming

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
                //.OnCreating(creating => creating.Create = () => new Zone())
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
            TagBuilder zoneWrapper = GetTagBuilder(Shape, "div");
            foreach (var item in Order(Shape))
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
                    foreach (var item in Order(tab))
                    {
                        tabBuilder.InnerHtml.Append(Display(item));
                    }
                    Output.Write(tabBuilder);
                }
            }
            else
            {
                foreach (var item in Order(unordered))
                {
                    Output.Write(Display(item));
                }
            }
        }

        [Shape]
        public void DocumentZone(dynamic Display, dynamic Shape, TextWriter Output)
        {
            foreach (var item in Order(Shape))
                Output.Write(Display(item));
        }

        public static IEnumerable<dynamic> Order(dynamic shape)
        {
            IEnumerable<dynamic> unordered = shape;
            if (unordered == null || unordered.Count() < 2)
                return shape;

            var i = 1;
            var progress = 1;
            var flatPositionComparer = new FlatPositionComparer();
            var ordering = unordered.Select(item => {
                string position = null;
                var itemPosition = item as IPositioned;
                if (itemPosition != null)
                {
                    position = itemPosition.Position;
                }

                return new { item, position };
            }).ToList();

            // since this isn't sticking around (hence, the "hack" in the name), throwing (in) a gnome 
            while (i < ordering.Count())
            {
                if (flatPositionComparer.Compare(ordering[i].position, ordering[i - 1].position) > -1)
                {
                    if (i == progress)
                        progress = ++i;
                    else
                        i = progress;
                }
                else
                {
                    var higherThanItShouldBe = ordering[i];
                    ordering[i] = ordering[i - 1];
                    ordering[i - 1] = higherThanItShouldBe;
                    if (i > 1)
                        --i;
                }
            }

            return ordering.Select(ordered => ordered.item).ToList();
        }

        static TagBuilder GetTagBuilder(dynamic shape, string defaultTag = "span")
        {
            string tagName = shape.Tag ?? defaultTag;
            string id = shape.Id;
            IEnumerable<string> classes = shape.Classes;
            IDictionary<string, string> attributes = shape.Attributes;

            return GetTagBuilder(tagName, id, classes, attributes);
        }

        static TagBuilder GetTagBuilder(string tagName, string id, IEnumerable<string> classes, IDictionary<string, string> attributes)
        {
            var tagBuilder = new TagBuilder(tagName);
            tagBuilder.MergeAttributes(attributes, false);

            foreach (var cssClass in classes ?? Enumerable.Empty<string>())
            {
                tagBuilder.AddCssClass(cssClass);
            }

            if (!string.IsNullOrWhiteSpace(id))
            {
                tagBuilder.Attributes["id"] = id;
            }
            return tagBuilder;
        }
    }
}