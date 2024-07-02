using System.Threading.Tasks;

namespace OrchardCore.DisplayManagement.Descriptors;

public interface IShapeTableProvider
{
    ValueTask DiscoverAsync(ShapeTableBuilder builder);
}
