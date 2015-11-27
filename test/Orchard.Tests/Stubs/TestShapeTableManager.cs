using Orchard.DisplayManagement.Descriptors;

namespace Orchard.Tests.Stubs
{
    public class TestShapeTableManager : IShapeTableManager
    {
        private readonly ShapeTable _defaultShapeTable;

        public TestShapeTableManager(ShapeTable defaultShapeTable)
        {
            _defaultShapeTable = defaultShapeTable;
        }

        public ShapeTable GetShapeTable(string themeName)
        {
            return _defaultShapeTable;
        }
    }
}