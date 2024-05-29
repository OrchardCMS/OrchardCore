using System.Threading.Tasks;
using Esprima.Ast;
using OrchardCore.DisplayManagement.Descriptors;

namespace OrchardCore.Testing.Stubs;

public class ShapeTableManagerStub : IShapeTableManager
{
    private readonly ShapeTable _defaultShapeTable;

    public ShapeTableManagerStub(ShapeTable defaultShapeTable)
    {
        _defaultShapeTable = defaultShapeTable;
    }

    public Task<ShapeTable> GetShapeTableAsync(string themeId) => Task.FromResult(_defaultShapeTable);
}
