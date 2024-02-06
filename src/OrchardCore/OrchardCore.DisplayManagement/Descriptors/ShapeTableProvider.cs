using System;
using System.Threading.Tasks;

namespace OrchardCore.DisplayManagement.Descriptors;

public abstract class ShapeTableProvider : IShapeTableProvider
{
    [Obsolete($"Instead, utilize the {nameof(IShapeTableProvider.DiscoverAsync)} method. This current method is slated for removal in upcoming releases.")]
    public void Discover(ShapeTableBuilder builder)
    {
    }

    public abstract ValueTask DiscoverAsync(ShapeTableBuilder builder);
}
