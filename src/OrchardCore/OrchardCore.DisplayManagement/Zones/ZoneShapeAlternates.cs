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
                    // Get cached alternate and add it efficiently
                    var cachedAlternates = ZoneAlternatesFactory.GetAlternates(zoneName);
                    context.Shape.Metadata.Alternates.AddRange(cachedAlternates);
                }
            });

        return ValueTask.CompletedTask;
    }
}
