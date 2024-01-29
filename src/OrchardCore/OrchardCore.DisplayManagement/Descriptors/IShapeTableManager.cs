using System;
using System.Threading.Tasks;

namespace OrchardCore.DisplayManagement.Descriptors;

public interface IShapeTableManager
{
    [Obsolete($"Instead, utilize the {nameof(GetShapeTableAsync)} method. This current method is slated for removal in upcoming releases.")]
    ShapeTable GetShapeTable(string themeId);

    Task<ShapeTable> GetShapeTableAsync(string themeId);
}
