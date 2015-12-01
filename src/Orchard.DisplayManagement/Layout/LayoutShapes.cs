using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Orchard.DisplayManagement.Descriptors;
using Orchard.UI;
using Orchard.Utility;
using Microsoft.AspNet.Mvc.Rendering;

// ReSharper disable InconsistentNaming

namespace Orchard.DisplayManagement.Zones
{
    public class LayoutShapes : IShapeTableProvider
    {
        public void Discover(ShapeTableBuilder builder)
        {

            // the root page shape named 'Layout' is wrapped with 'Document'
            // and has an automatic zone creating behavior
            builder.Describe("Layout")
                //.Configure(descriptor => descriptor.Wrappers.Add("Document"))
                .OnCreating(creating => creating.Create = () => new ZoneHolding(() => creating.New.Zone()))
                .OnCreated(created => {
                    var layout = created.Shape;

                    layout.Head = created.New.DocumentZone(ZoneName: "Head");
                    layout.Body = created.New.DocumentZone(ZoneName: "Body");
                    layout.Tail = created.New.DocumentZone(ZoneName: "Tail");

                    //layout.Body.Add(created.New.PlaceChildContent(Source: layout));

                    layout.Content = created.New.Zone();
                    layout.Content.ZoneName = "Content";
                    //layout.Content.Add(created.New.PlaceChildContent(Source: layout));

                });
        }
   }
}