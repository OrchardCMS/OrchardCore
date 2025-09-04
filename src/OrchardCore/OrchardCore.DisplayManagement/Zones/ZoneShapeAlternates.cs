using OrchardCore.DisplayManagement.Descriptors;

namespace OrchardCore.DisplayManagement.Zones;

public class ZoneShapeAlternates : ShapeTableProvider
{
    public override ValueTask DiscoverAsync(ShapeTableBuilder builder)
    {
        builder.Describe("Zone")
            .OnDisplaying(context =>
            {
                if (context.Shape.TryGetProperty("ZoneName", out string zoneName))
                {
                    context.Shape.Metadata.Alternates.Add("Zone__" + zoneName);
                }
            });

        return ValueTask.CompletedTask;
    }
}
