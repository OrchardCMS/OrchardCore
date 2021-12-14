using OrchardCore.DisplayManagement.Descriptors;

namespace OrchardCore.Stubs.Tests
{
    public class TestShapeTableManager : IShapeTableManager
    {
        private readonly ShapeTable _defaultShapeTable;

        public TestShapeTableManager(ShapeTable defaultShapeTable)
        {
            _defaultShapeTable = defaultShapeTable;
        }

        public ShapeTable GetShapeTable(string themeId)
        {
            return _defaultShapeTable;
        }
    }
}
