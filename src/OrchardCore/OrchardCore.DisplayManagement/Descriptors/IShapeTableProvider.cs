using System;
using System.Threading.Tasks;

namespace OrchardCore.DisplayManagement.Descriptors;

public interface IShapeTableProvider
{
    [Obsolete($"Instead, utilize the {nameof(DiscoverAsync)} method. This current method is slated for removal in upcoming releases.")]
    void Discover(ShapeTableBuilder builder);

    ValueTask DiscoverAsync(ShapeTableBuilder builder)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        Discover(builder);
#pragma warning restore CS0618 // Type or member is obsolete

        return ValueTask.CompletedTask;
    }
}
