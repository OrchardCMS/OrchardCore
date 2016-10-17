using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Theming;
using System.Threading.Tasks;
using Orchard.Environment.Extensions.Models;
using System;

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