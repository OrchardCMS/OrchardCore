using OrchardCore.DisplayManagement.Descriptors;

namespace OrchardCore.Testing.Stubs;

public class ShapeTableManagerStub : IShapeTableManager
{
    private readonly ShapeTable _defaultShapeTable;

    public ShapeTableManagerStub(ShapeTable defaultShapeTable)
    {
        _defaultShapeTable = defaultShapeTable;
    }

    public ShapeTable GetShapeTable(string themeId) => _defaultShapeTable;
}
