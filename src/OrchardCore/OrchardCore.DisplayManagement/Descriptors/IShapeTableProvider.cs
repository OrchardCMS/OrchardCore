using OrchardCore.Environment.Extensions;

namespace OrchardCore.DisplayManagement.Descriptors;

[FeatureTypeDiscovery(SkipExtension = true)]
public interface IShapeTableProvider
{
    ValueTask DiscoverAsync(ShapeTableBuilder builder);
}
