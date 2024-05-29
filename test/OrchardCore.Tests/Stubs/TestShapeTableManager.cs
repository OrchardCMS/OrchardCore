using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.Environment.Extensions;

namespace OrchardCore.Tests.Stubs;

public class TestShapeTableManager(ShapeTable defaultShapeTable) : IShapeTableManager
{
    private readonly ShapeTable _defaultShapeTable = defaultShapeTable;

    public ShapeTable GetShapeTable(string themeId)
        => _defaultShapeTable;

    public Task<ShapeTable> GetShapeTableAsync(string themeId)
        => Task.FromResult(_defaultShapeTable);
}

public class MockThemeManager(IExtensionInfo des) : IThemeManager
{
    private readonly IExtensionInfo _dec = des;

    public Task<IExtensionInfo> GetThemeAsync()
    {
        return Task.FromResult(_dec);
    }
}
