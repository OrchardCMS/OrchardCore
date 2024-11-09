using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Layout;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Extensions;
using OrchardCore.Scripting;
using OrchardCore.Tests.DisplayManagement.Stubs;
using OrchardCore.Tests.Stubs;

namespace OrchardCore.Tests.DisplayManagement;

public class ShapeResultTests
{
    [Theory]
    [InlineData("groupOne", "gRoUpOnE")] // case insensitive check.
    [InlineData("groupOne", "groupOne")]
    [InlineData("", "")]
    [InlineData("", null)]
    [InlineData(null, "")]
    [InlineData(null, null)]
    public async Task Shape_WhenCalled_ReturnShapeWhenGroupIsMatched(string groupId, string renderingGroupId)
    {
        var serviceProvider = GetServiceProvider(new GroupDisplayDriverStub(groupId));

        var displayManager = serviceProvider.GetRequiredService<IDisplayManager<GroupModel>>();
        var model = new GroupModel();

        var shape = await displayManager.BuildEditorAsync(model, updater: null, isNew: false, groupId: renderingGroupId);

        var testZone = shape.GetProperty<IShape>(GroupDisplayDriverStub.ZoneName);

        Assert.NotNull(testZone);

        var shapeModel = testZone.Items[0] as ShapeViewModel<GroupModel>;

        Assert.NotNull(shapeModel);
        Assert.Equal(shapeModel.Value.Value, model.Value);
    }

    [Theory]
    [InlineData("groupOne", "groupTwo")]
    [InlineData("", "groupTwo")]
    [InlineData(null, "groupTwo")]
    [InlineData("groupOne", "")]
    [InlineData("groupOne", null)]
    public async Task Shape_WhenCalled_ReturnNullWhenIncorrectGroupIsSpecified(string groupId, string renderingGroupId)
    {
        var serviceProvider = GetServiceProvider(new GroupDisplayDriverStub(groupId));

        var displayManager = serviceProvider.GetRequiredService<IDisplayManager<GroupModel>>();
        var model = new GroupModel();

        var shape = await displayManager.BuildEditorAsync(model, updater: null, isNew: false, groupId: renderingGroupId);

        var testZone = shape.GetProperty<IShape>(GroupDisplayDriverStub.ZoneName);

        Assert.Null(testZone);
    }

    private static ServiceProvider GetServiceProvider(IDisplayDriver<GroupModel> driver)
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddScripting()
            .AddLogging()
            .AddScoped<ILoggerFactory, NullLoggerFactory>()
            .AddScoped<IThemeManager, ThemeManager>()
            .AddScoped<IShapeFactory, DefaultShapeFactory>()
            .AddScoped<IExtensionManager, StubExtensionManager>()
            .AddScoped<IShapeTableManager, TestShapeTableManager>()
            .AddScoped<ILayoutAccessor, LayoutAccessor>()
            .AddScoped(sp => driver)
            .AddScoped(typeof(IDisplayManager<>), typeof(DisplayManager<>));

        var shapeTable = new ShapeTable
        (
            new Dictionary<string, ShapeDescriptor>(StringComparer.OrdinalIgnoreCase),
            new Dictionary<string, ShapeBinding>(StringComparer.OrdinalIgnoreCase)
        );

        serviceCollection.AddSingleton(shapeTable);

        return serviceCollection.BuildServiceProvider();
    }
}
