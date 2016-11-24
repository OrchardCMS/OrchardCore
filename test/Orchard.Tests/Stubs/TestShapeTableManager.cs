using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.Theming;
using System.Threading.Tasks;
using System;
using Orchard.Environment.Extensions;

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
        IExtensionInfo _dec;
        public MockThemeManager(IExtensionInfo des) {
            _dec = des;
        }
        public Task<IExtensionInfo> GetThemeAsync()
        {
            return Task.Run(() => _dec);
        }
    }
}