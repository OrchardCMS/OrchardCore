using OrchardCore.Admin;
using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.Environment.Extensions.Features;

namespace OrchardCore.Tests.Modules.OrchardCore.Admin;

public class AdminListShapeTableProviderTests
{
    [Fact]
    public async Task AdminList_AddsLayoutAndNameAlternates_MostSpecificLast()
    {
        var shape = new Shape();
        shape.Metadata.Type = AdminListConstants.ShapeType;
        shape.Properties["Name"] = "Contents";
        shape.Properties["Layout"] = AdminListConstants.Grid;

        await DisplayAsync(AdminListConstants.ShapeType, shape);

        Assert.Equal(
            ["AdminList__Grid", "AdminList__Contents", "AdminList__Contents__Grid"],
            shape.Metadata.Alternates.ToArray());
    }

    [Fact]
    public async Task AdminList_DefaultsToListLayout_WhenLayoutIsMissing()
    {
        var shape = new Shape();
        shape.Metadata.Type = AdminListConstants.ShapeType;

        await DisplayAsync(AdminListConstants.ShapeType, shape);

        Assert.Equal(["AdminList__List"], shape.Metadata.Alternates.ToArray());
    }

    [Fact]
    public async Task AdminListActions_AddsLayoutAndListAlternates()
    {
        var shape = new Shape();
        shape.Metadata.Type = AdminListActionsLayouts.ShapeType;
        shape.Properties["ListName"] = "Contents";
        shape.Properties["Layout"] = AdminListActionsLayouts.Menu;

        await DisplayAsync(AdminListActionsLayouts.ShapeType, shape);

        Assert.Equal(
            ["AdminListActions__Menu", "AdminListActions__Contents", "AdminListActions__Contents__Menu"],
            shape.Metadata.Alternates.ToArray());
    }

    [Fact]
    public async Task AdminListActions_DefaultsToButtons_WithoutLayoutOrServices()
    {
        var shape = new Shape();
        shape.Metadata.Type = AdminListActionsLayouts.ShapeType;

        await DisplayAsync(AdminListActionsLayouts.ShapeType, shape);

        Assert.Equal(AdminListActionsLayouts.Buttons, shape.Properties["Layout"]);
        Assert.Equal(["AdminListActions__Buttons"], shape.Metadata.Alternates.ToArray());
    }

    [Fact]
    public async Task AdminList_StampsItsNameOnEveryPartOfTheList()
    {
        var toolbar = new Shape();
        var search = new Shape();
        var pager = new Shape();
        var row = new Shape();
        var namedRow = new Shape();
        namedRow.Properties["ListName"] = "SomethingElse";

        var shape = new Shape();
        shape.Metadata.Type = AdminListConstants.ShapeType;
        shape.Properties["Name"] = "Contents";
        shape.Properties["Layout"] = AdminListConstants.Grid;
        shape.Properties["Toolbar"] = toolbar;
        shape.Properties["Search"] = search;
        shape.Properties["Pager"] = pager;
        shape.Properties["Rows"] = new List<IShape> { row, namedRow };

        await DisplayAsync(AdminListConstants.ShapeType, shape);

        Assert.Equal("Contents", toolbar.Properties["ListName"]);
        Assert.Equal("Contents", search.Properties["ListName"]);
        Assert.Equal("Contents", pager.Properties["ListName"]);
        Assert.Equal("Contents", row.Properties["ListName"]);

        // The layout rendering the list is stamped as well, so a part can be overridden for one layout.
        Assert.Equal(AdminListConstants.Grid, toolbar.Properties["ListLayout"]);
        Assert.Equal(AdminListConstants.Grid, search.Properties["ListLayout"]);
        Assert.Equal(AdminListConstants.Grid, row.Properties["ListLayout"]);

        // A part that already names a list keeps it, e.g. a list rendered inside another one.
        Assert.Equal("SomethingElse", namedRow.Properties["ListName"]);
    }

    [Fact]
    public async Task AdminListToolbarAndSearch_AddTheLayoutAndListAlternates()
    {
        var toolbar = new Shape();
        toolbar.Metadata.Type = AdminListConstants.ToolbarShapeType;
        toolbar.Properties["ListName"] = "Contents";
        toolbar.Properties["ListLayout"] = AdminListConstants.Grid;

        await DisplayAsync(AdminListConstants.ToolbarShapeType, toolbar);

        Assert.Equal(
            ["AdminListToolbar__Grid", "AdminListToolbar__Contents", "AdminListToolbar__Contents__Grid"],
            toolbar.Metadata.Alternates.ToArray());

        var search = new Shape();
        search.Metadata.Type = AdminListConstants.SearchShapeType;
        search.Properties["ListName"] = "Contents";
        search.Properties["ListLayout"] = AdminListConstants.Grid;

        await DisplayAsync(AdminListConstants.SearchShapeType, search);

        Assert.Equal(
            ["AdminListSearch__Grid", "AdminListSearch__Contents", "AdminListSearch__Contents__Grid"],
            search.Metadata.Alternates.ToArray());
    }

    [Fact]
    public async Task AdminListToolbar_AddsTheLayoutAlternate_WithoutAList()
    {
        var toolbar = new Shape();
        toolbar.Metadata.Type = AdminListConstants.ToolbarShapeType;
        toolbar.Properties["ListLayout"] = AdminListConstants.List;

        await DisplayAsync(AdminListConstants.ToolbarShapeType, toolbar);

        Assert.Equal(["AdminListToolbar__List"], toolbar.Metadata.Alternates.ToArray());
    }

    [Fact]
    public async Task AdminListActions_TakesTheListNameFromItsRow()
    {
        // A row template renders the actions without naming the list, which the row carries.
        var row = new Shape();
        row.Properties["ListName"] = "Contents";

        var shape = new Shape();
        shape.Metadata.Type = AdminListActionsLayouts.ShapeType;
        shape.Properties["Row"] = row;
        shape.Properties["Layout"] = AdminListActionsLayouts.Menu;

        await DisplayAsync(AdminListActionsLayouts.ShapeType, shape);

        Assert.Equal(
            ["AdminListActions__Menu", "AdminListActions__Contents", "AdminListActions__Contents__Menu"],
            shape.Metadata.Alternates.ToArray());
    }

    [Fact]
    public async Task AdminListCell_AddsColumnAndListAlternates()
    {
        var shape = new Shape();
        shape.Metadata.Type = AdminListConstants.CellShapeType;
        shape.Properties["ListName"] = "Contents";
        shape.Properties["Column"] = new AdminListColumn { Name = "Actions" };

        await DisplayAsync(AdminListConstants.CellShapeType, shape);

        Assert.Equal(
            ["AdminListCell__Actions", "AdminListCell__Contents__Actions"],
            shape.Metadata.Alternates.ToArray());
    }

    [Fact]
    public async Task AdminListCell_AddsNoAlternates_WithoutColumn()
    {
        var shape = new Shape();
        shape.Metadata.Type = AdminListConstants.CellShapeType;

        await DisplayAsync(AdminListConstants.CellShapeType, shape);

        Assert.Empty(shape.Metadata.Alternates);
    }

    [Fact]
    public async Task AdminListActions_WithoutLayout_UsesTheConfiguredActionsLayout()
    {
        var shape = new Shape();
        shape.Metadata.Type = AdminListActionsLayouts.ShapeType;

        var services = new ServiceCollection()
            .AddSingleton(Mock.Of<IOptionsMonitor<AdminListOptions>>(m => m.CurrentValue == new AdminListOptions { DefaultActionsLayout = AdminListActionsLayouts.Menu }))
            .BuildServiceProvider();

        await DisplayAsync(AdminListActionsLayouts.ShapeType, shape, services);

        Assert.Equal(AdminListActionsLayouts.Menu, shape.Properties["Layout"]);
        Assert.Equal(["AdminListActions__Menu"], shape.Metadata.Alternates.ToArray());
    }

    [Fact]
    public async Task AdminList_SiteLetsUsersChoose_OffersTheOtherLayouts()
    {
        var shape = CreateNamedList();

        await DisplayAsync(AdminListConstants.ShapeType, shape, CreateSelectorServices());

        var selector = Assert.IsAssignableFrom<IShape>(shape.Properties["LayoutSelector"]);
        Assert.Equal("Contents", selector.Properties["ListName"]);
        Assert.Equal(AdminListConstants.Grid, selector.Properties["Current"]);
        Assert.Equal(2, ((IEnumerable<AdminListLayoutOption>)selector.Properties["Layouts"]).Count());
    }

    [Fact]
    public async Task AdminList_PageTurnsTheSelectorOff_OffersNoLayout()
    {
        var shape = CreateNamedList();
        shape.Properties["ShowLayoutSelector"] = false;

        await DisplayAsync(AdminListConstants.ShapeType, shape, CreateSelectorServices());

        Assert.False(shape.Properties.ContainsKey("LayoutSelector"));
    }

    private static Shape CreateNamedList()
    {
        var shape = new Shape();
        shape.Metadata.Type = AdminListConstants.ShapeType;
        shape.Properties["Name"] = "Contents";
        shape.Properties["Layout"] = AdminListConstants.Grid;

        return shape;
    }

    private static ServiceProvider CreateSelectorServices()
    {
        var layoutResolver = new Mock<IAdminListLayoutResolver>();
        layoutResolver
            .Setup(r => r.GetLayoutOptionsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AdminListLayoutOption>
            {
                new() { Name = AdminListConstants.List, Url = "?layout=List" },
                new() { Name = AdminListConstants.Grid, Url = "?layout=Grid" },
            });

        return new ServiceCollection()
            .AddSingleton(layoutResolver.Object)
            .AddSingleton<IShapeFactory, TestShapeFactory>()
            .BuildServiceProvider();
    }

    private static async Task DisplayAsync(string shapeType, Shape shape, IServiceProvider services = null)
    {
        var feature = new Mock<IFeatureInfo>();
        feature.Setup(f => f.Id).Returns("OrchardCore.Admin");

        var builder = new ShapeTableBuilder(feature.Object);
        await new AdminListShapeTableProvider().DiscoverAsync(builder);

        var descriptor = new ShapeDescriptor { ShapeType = shapeType };

        foreach (var alteration in builder.BuildAlterations().Where(a => a.ShapeType == shapeType))
        {
            alteration.Alter(descriptor);
        }

        var context = new ShapeDisplayContext { Shape = shape, ServiceProvider = services };

        foreach (var displaying in descriptor.DisplayingAsync)
        {
            await displaying(context);
        }
    }

    private sealed class TestShapeFactory : IShapeFactory
    {
        public dynamic New => this;

        public async ValueTask<IShape> CreateAsync(
            string shapeType,
            Func<ValueTask<IShape>> shapeFactory,
            Action<ShapeCreatingContext> creating,
            Action<ShapeCreatedContext> created)
        {
            var shape = await shapeFactory();
            shape.Metadata.Type = shapeType;

            created?.Invoke(new ShapeCreatedContext
            {
                Shape = shape,
                ShapeFactory = this,
                ShapeType = shapeType,
            });

            return shape;
        }
    }
}
