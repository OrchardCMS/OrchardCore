using Orchard.DisplayManagement.Descriptors;

namespace Orchard.DisplayManagement.Zones
{
    public class LayoutShapes : IShapeTableProvider
    {
        public void Discover(ShapeTableBuilder builder)
        {
            builder
                .Describe("Layout")
                .OnCreating(creating => creating.Create = () => new ZoneHolding(() => creating.New.Zone()))
                .OnCreated(created => 
                {
                    var layout = created.Shape;

                    layout.Head = created.New.DocumentZone(ZoneName: "Head");
                    layout.Body = created.New.DocumentZone(ZoneName: "Body");
                    layout.Tail = created.New.DocumentZone(ZoneName: "Tail");

                    layout.Content = created.New.Zone();
                    layout.Content.ZoneName = "Content";
                });
        }
   }
}