using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Descriptors;

namespace OrchardCore.DisplayManagement.Zones
{
    public class LayoutShapes : IShapeTableProvider
    {
        public void Discover(ShapeTableBuilder builder)
        {
            builder
                .Describe("Layout")
                .OnCreating(creating =>
                {
                    creating.CreateAsync = () => Task.FromResult<IShape>(new ZoneHolding(() => creating.ShapeFactory.CreateAsync("Zone")));
                })
                .OnCreated(async created => 
                {
                    dynamic layout = created.Shape;

                    layout.Head = await created.ShapeFactory.CreateAsync("DocumentZone", new { ZoneName = "Head" });
                    layout.Body = await created.ShapeFactory.CreateAsync("DocumentZone", new { ZoneName = "Body" });
                    layout.Tail = await created.ShapeFactory.CreateAsync("DocumentZone", new { ZoneName = "Tail" });

                    layout.Content = await created.ShapeFactory.CreateAsync("Zone", new { ZoneName = "Content" });
                });
        }
   }
}