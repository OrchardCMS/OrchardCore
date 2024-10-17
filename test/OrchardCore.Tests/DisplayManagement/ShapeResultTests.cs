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
    [Fact]
    public async Task Shape_WhenCalled_ReturnShapeWhenNoGroupIsProvided()
    {
        var serviceProvider = GetServiceProvider(new GroupDisplayDriverStub());

        var displayManager = serviceProvider.GetRequiredService<IDisplayManager<GroupModel>>();
        var model = new GroupModel();

        var shape = await displayManager.BuildEditorAsync(model, updater: null, isNew: false);

        var testZone = shape.GetProperty<IShape>(GroupDisplayDriverStub.ZoneName);

        Assert.NotNull(testZone);

        var shapeModel = testZone.Items[0] as ShapeViewModel<GroupModel>;

        Assert.NotNull(shapeModel);
        Assert.Equal(shapeModel.Value.Value, model.Value);
    }

    [Fact]
    public async Task Shape_WhenCalled_ReturnShapeWhenGroupIsMatched()
    {
        var groupId = "abc";
        var serviceProvider = GetServiceProvider(new GroupDisplayDriverStub(groupId));

        var displayManager = serviceProvider.GetRequiredService<IDisplayManager<GroupModel>>();
        var model = new GroupModel();

        var shape = await displayManager.BuildEditorAsync(model, updater: null, isNew: false, groupId: groupId);

        var testZone = shape.GetProperty<IShape>(GroupDisplayDriverStub.ZoneName);

        Assert.NotNull(testZone);

        var shapeModel = testZone.Items[0] as ShapeViewModel<GroupModel>;

        Assert.NotNull(shapeModel);
        Assert.Equal(shapeModel.Value.Value, model.Value);
    }

    [Fact]
    public async Task Shape_WhenCalled_NullGroupShouldBeTreatedAsEmptyString()
    {
        var serviceProvider = GetServiceProvider(new GroupDisplayDriverStub(""));

        var displayManager = serviceProvider.GetRequiredService<IDisplayManager<GroupModel>>();
        var model = new GroupModel();

        var shape = await displayManager.BuildEditorAsync(model, updater: null, isNew: false, groupId: null);

        var testZone = shape.GetProperty<IShape>(GroupDisplayDriverStub.ZoneName);

        Assert.NotNull(testZone);

        var shapeModel = testZone.Items[0] as ShapeViewModel<GroupModel>;

        Assert.NotNull(shapeModel);
        Assert.Equal(shapeModel.Value.Value, model.Value);
    }

    [Fact]
    public async Task Shape_WhenCalled_ReturnShapeWhenMatchedToAnyGroup()
    {
        var groupId = "abc";
        var serviceProvider = GetServiceProvider(new GroupDisplayDriverStub("xyz", "test", groupId));

        var displayManager = serviceProvider.GetRequiredService<IDisplayManager<GroupModel>>();
        var model = new GroupModel();

        var shape = await displayManager.BuildEditorAsync(model, updater: null, isNew: false, groupId: groupId);

        var testZone = shape.GetProperty<IShape>(GroupDisplayDriverStub.ZoneName);

        Assert.NotNull(testZone);

        var shapeModel = testZone.Items[0] as ShapeViewModel<GroupModel>;

        Assert.NotNull(shapeModel);
        Assert.Equal(shapeModel.Value.Value, model.Value);
    }

    [Fact]
    public async Task Shape_WhenCalled_ReturnShapeWhenMatchedToAnyGroupCaseInsensitive()
    {
        var serviceProvider = GetServiceProvider(new GroupDisplayDriverStub("xyz", "test"));

        var displayManager = serviceProvider.GetRequiredService<IDisplayManager<GroupModel>>();
        var model = new GroupModel();

        var shape = await displayManager.BuildEditorAsync(model, updater: null, isNew: false, groupId: "xYz");

        var testZone = shape.GetProperty<IShape>(GroupDisplayDriverStub.ZoneName);

        Assert.NotNull(testZone);

        var shapeModel = testZone.Items[0] as ShapeViewModel<GroupModel>;

        Assert.NotNull(shapeModel);
        Assert.Equal(shapeModel.Value.Value, model.Value);
    }

    [Fact]
    public async Task Shape_WhenCalled_ReturnNullWhenIncorrectGroupIsSpecified()
    {
        var serviceProvider = GetServiceProvider(new GroupDisplayDriverStub("groupOne"));

        var displayManager = serviceProvider.GetRequiredService<IDisplayManager<GroupModel>>();
        var model = new GroupModel();

        var shape = await displayManager.BuildEditorAsync(model, updater: null, isNew: false, groupId: "groupTwo");

        var testZone = shape.GetProperty<IShape>(GroupDisplayDriverStub.ZoneName);

        Assert.Null(testZone);
    }

    [Fact]
    public async Task Shape_WhenCalled_ReturnNullWhenGroupDoesNotMatchExactGroup()
    {
        var serviceProvider = GetServiceProvider(new GroupDisplayDriverStub());

        var displayManager = serviceProvider.GetRequiredService<IDisplayManager<GroupModel>>();
        var model = new GroupModel();

        var shape = await displayManager.BuildEditorAsync(model, updater: null, isNew: false, groupId: "groupTwo");

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
