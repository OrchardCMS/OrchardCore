using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.Environment.Extensions;

namespace OrchardCore.Tests.Stubs
{
    public class TestShapeTable : ShapeTable
    {
        public override IDictionary<string, ShapeBinding> Bindings { get; set; }
    }

    public class TestShapeTableManager : IShapeTableManager
    {
        private readonly TestShapeTable _defaultShapeTable;

        public TestShapeTableManager(TestShapeTable defaultShapeTable)
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
        IExtensionInfo _dec;
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