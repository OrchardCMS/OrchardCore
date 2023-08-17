using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.Environment.Extensions;

namespace OrchardCore.Tests.Stubs
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

    public class MockThemeManager : IThemeManager
    {
        private readonly IExtensionInfo _dec;
        public MockThemeManager(IExtensionInfo des)
        {
            _dec = des;
        }
        public Task<IExtensionInfo> GetThemeAsync()
        {
            return Task.FromResult(_dec);
        }
    }
}
