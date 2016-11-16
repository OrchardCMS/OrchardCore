using System.Collections.Generic;
using System.Threading.Tasks;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Theming;
using Orchard.Environment.Extensions.Models;

namespace Orchard.Tests.Stubs
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

        public ShapeTable GetShapeTable(string themeName)
        {
            return _defaultShapeTable;
        }
    }

    public class MockThemeManager : IThemeManager
    {
        ExtensionDescriptor _dec;
        public MockThemeManager(ExtensionDescriptor des) {
            _dec = des;
        }
        public Task<ExtensionDescriptor> GetThemeAsync()
        {
            return Task.Run(() => _dec);
        }
    }
}