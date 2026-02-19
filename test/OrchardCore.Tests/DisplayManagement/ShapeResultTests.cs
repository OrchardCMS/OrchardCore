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

    [Fact]
    public async Task Shape_WhenRenderPredicateReturnsFalse_DoesNotRenderShape()
    {
        var serviceProvider = GetServiceProvider(new RenderPredicateDisplayDriverStub(canRender: false));

        var displayManager = serviceProvider.GetRequiredService<IDisplayManager<GroupModel>>();
        var model = new GroupModel();

        var shape = await displayManager.BuildEditorAsync(model, updater: null, isNew: false);

        var testZone = shape.GetProperty<IShape>(GroupDisplayDriverStub.ZoneName);

        Assert.Null(testZone);
    }

    [Fact]
    public async Task Shape_WhenTypedRenderPredicateReturnsTrue_RendersShape()
    {
        var serviceProvider = GetServiceProvider(new TypedRenderPredicateDisplayDriverStub(canRender: true));

        var displayManager = serviceProvider.GetRequiredService<IDisplayManager<GroupModel>>();
        var model = new GroupModel();

        var shape = await displayManager.BuildEditorAsync(model, updater: null, isNew: false);

        var testZone = shape.GetProperty<IShape>(GroupDisplayDriverStub.ZoneName);

        Assert.NotNull(testZone);
        Assert.IsType<ShapeViewModel<GroupModel>>(testZone.Items[0]);
    }

    [Fact]
    public async Task Shape_WhenObjectStateRenderPredicateReturnsTrue_RendersShape()
    {
        var serviceProvider = GetServiceProvider(new ObjectStateRenderPredicateDisplayDriverStub(canRender: true));

        var displayManager = serviceProvider.GetRequiredService<IDisplayManager<GroupModel>>();
        var model = new GroupModel();

        var shape = await displayManager.BuildEditorAsync(model, updater: null, isNew: false);

        var testZone = shape.GetProperty<IShape>(GroupDisplayDriverStub.ZoneName);

        Assert.NotNull(testZone);
        Assert.IsType<ShapeViewModel<GroupModel>>(testZone.Items[0]);
    }

    private sealed class RenderPredicateDisplayDriverStub : DisplayDriver<GroupModel>
    {
        private readonly bool _canRender;

        public RenderPredicateDisplayDriverStub(bool canRender)
        {
            _canRender = canRender;
        }

        public override IDisplayResult Edit(GroupModel model, BuildEditorContext context)
            => View("test", model)
                .Location(GroupDisplayDriverStub.ZoneName)
                .RenderWhen(() => Task.FromResult(_canRender));
    }

    private sealed class TypedRenderPredicateDisplayDriverStub : DisplayDriver<GroupModel>
    {
        private readonly bool _canRender;

        public TypedRenderPredicateDisplayDriverStub(bool canRender)
        {
            _canRender = canRender;
        }

        public override IDisplayResult Edit(GroupModel model, BuildEditorContext context)
            => View("test", model)
                .Location(GroupDisplayDriverStub.ZoneName)
                .RenderWhen(() => Task.FromResult(_canRender));
    }

    private sealed class ObjectStateRenderPredicateDisplayDriverStub : DisplayDriver<GroupModel>
    {
        private readonly bool _canRender;

        public ObjectStateRenderPredicateDisplayDriverStub(bool canRender)
        {
            _canRender = canRender;
        }

        public override IDisplayResult Edit(GroupModel model, BuildEditorContext context)
            => View("test", model)
                .Location(GroupDisplayDriverStub.ZoneName)
                .RenderWhen(static state => Task.FromResult((bool)state!), _canRender);
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
