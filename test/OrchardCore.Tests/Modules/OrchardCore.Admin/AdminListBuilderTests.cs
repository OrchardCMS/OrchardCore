using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;
using OrchardCore.Admin.Services;
using OrchardCore.Admin.TagHelpers;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.Tests.Modules.OrchardCore.Admin;

public class AdminListBuilderTests
{
    [Theory]
    [InlineData(AdminListLayout.List, true, "SummaryAdmin")]
    [InlineData(AdminListLayout.Grid, true, "SummaryAdminGrid")]
    [InlineData(AdminListLayout.Grid, false, "SummaryAdmin")]
    public async Task BuildAsync_ContextItems_BuildsSelectedDisplayTypeOnce(AdminListLayout layout, bool hasColumns, string displayType)
    {
        var row = new Shape();
        var display = new Mock<IAdminListItemDisplayManager<string>>();
        display.Setup(manager => manager.BuildDisplayAsync("item", It.IsAny<IUpdateModel>(), displayType))
            .ReturnsAsync(row);
        using var services = CreateServices(display.Object);
        var (builder, _, _) = CreateBuilder(services, hasColumns ? _columns : [], layout);
        var shape = await CreateListAsync(new AdminListContext<string> { Source = ["item"] });
        display.VerifyNoOtherCalls();

        await builder.BuildAsync(shape, TestContext.Current.CancellationToken);
        await builder.BuildAsync(shape, TestContext.Current.CancellationToken);

        Assert.Same(row, Assert.Single(shape.GetProperty<IEnumerable<IShape>>("Rows")));
        Assert.Equal(displayType, shape.GetProperty<string>("DisplayType"));
        Assert.Equal(1, shape.GetProperty<IShape>("Toolbar").Properties["ItemsCount"]);
        display.Verify(manager => manager.BuildDisplayAsync("item", It.IsAny<IUpdateModel>(), displayType), Times.Once);
    }

    [Fact]
    public async Task BuildAsync_DisplayManagerFailure_PropagatesWithoutInitializing()
    {
        var display = new Mock<IAdminListItemDisplayManager<string>>();
        display.Setup(manager => manager.BuildDisplayAsync("item", It.IsAny<IUpdateModel>(), "SummaryAdmin"))
            .ThrowsAsync(new InvalidOperationException("Row failed"));
        using var services = CreateServices(display.Object);
        var (builder, _, _) = CreateBuilder(services, [], AdminListLayout.List);
        var shape = await CreateListAsync(new AdminListContext<string> { Source = ["item"] });

        await Assert.ThrowsAsync<InvalidOperationException>(() => builder.BuildAsync(shape, TestContext.Current.CancellationToken));

        Assert.False(shape.GetProperty<bool>("AdminListInitialized"));
    }

    [Fact]
    public async Task BuildAsync_UndefinedLayout_Throws()
    {
        using var services = CreateServices();
        var (builder, _, _) = CreateBuilder(services, _columns, AdminListLayout.List);
        var shape = await CreateListAsync(new AdminListContext { Layout = (AdminListLayout)42 });

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => builder.BuildAsync(shape, TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData(AdminListLayout.List, false, false)]
    [InlineData(AdminListLayout.Grid, false, true)]
    [InlineData(AdminListLayout.List, true, true)]
    public async Task BuildAsync_LayoutAndSelection_BuildsColumnsOnlyWhenNeeded(AdminListLayout layout, bool allowSelection, bool buildsColumns)
    {
        using var services = CreateServices();
        var (builder, _, columnsBuilder) = CreateBuilder(services, _columns, layout, allowSelection);
        var shape = await CreateListAsync(new AdminListContext());
        var columns = new[] { new AdminListColumn { Name = "FromView" } };
        shape.Properties["Columns"] = columns;

        await builder.BuildAsync(shape, TestContext.Current.CancellationToken);
        await builder.BuildAsync(shape, TestContext.Current.CancellationToken);

        Assert.Equal(layout, shape.Properties["Layout"]);
        columnsBuilder.Verify(value => value.BuildColumnsAsync("Contents", columns,
            It.IsAny<IReadOnlyDictionary<string, object>>(), TestContext.Current.CancellationToken),
            buildsColumns ? Times.Once() : Times.Never());
        Assert.Equal(buildsColumns, shape.Properties["ShowLayoutSelector"]);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task BuildAsync_NoColumns_FallsBackWithoutSavingPreference(bool allowSelection)
    {
        using var services = CreateServices();
        var (builder, http, _) = CreateBuilder(services, [], AdminListLayout.Grid, allowSelection);
        var shape = await CreateListAsync(new AdminListContext());

        await builder.BuildAsync(shape, TestContext.Current.CancellationToken);

        Assert.Equal(AdminListLayout.List, shape.Properties["Layout"]);
        Assert.Equal(false, shape.Properties["ShowLayoutSelector"]);
        Assert.False(http.Response.Headers.ContainsKey("Set-Cookie"));
    }

    [Fact]
    public async Task BuildAsync_ForcedListWithoutSelector_SkipsColumnsAndPreference()
    {
        using var services = CreateServices();
        var (builder, http, columnsBuilder) = CreateBuilder(services, _columns, AdminListLayout.Grid, true);
        var shape = await CreateListAsync(new AdminListContext
        {
            Layout = AdminListLayout.List,
            ShowLayoutSelector = false,
        });

        await builder.BuildAsync(shape, TestContext.Current.CancellationToken);

        Assert.Equal(AdminListLayout.List, shape.Properties["Layout"]);
        Assert.Equal(false, shape.Properties["ShowLayoutSelector"]);
        Assert.False(http.Response.Headers.ContainsKey("Set-Cookie"));
        columnsBuilder.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task BuildAsync_SharedContext_PassesEachNameAndDataToProviders()
    {
        using var services = CreateServices();
        var (builder, _, columnsBuilder) = CreateBuilder(services, _columns, AdminListLayout.Grid);
        var context = new AdminListContext();
        context.Data["ContentTypes"] = new[] { "BlogPost" };
        var first = await CreateListAsync(context, "First");
        var second = await CreateListAsync(context, "Second");

        await builder.BuildAsync(first, TestContext.Current.CancellationToken);
        await builder.BuildAsync(second, TestContext.Current.CancellationToken);

        Assert.NotSame(first, second);
        foreach (var name in new[] { "First", "Second" })
        {
            columnsBuilder.Verify(value => value.BuildColumnsAsync(name, It.IsAny<IEnumerable<AdminListColumn>>(),
                context.Data, TestContext.Current.CancellationToken), Times.Once);
        }
    }

    [Fact]
    public async Task BuildAsync_Grid_PreservesRowOrderWithoutCellCopies()
    {
        var first = new Shape();
        var second = new Shape();
        var display = new Mock<IAdminListItemDisplayManager<string>>();
        display.Setup(manager => manager.BuildDisplayAsync("first", It.IsAny<IUpdateModel>(), "SummaryAdminGrid")).ReturnsAsync(first);
        display.Setup(manager => manager.BuildDisplayAsync("second", It.IsAny<IUpdateModel>(), "SummaryAdminGrid")).ReturnsAsync(second);
        using var services = CreateServices(display.Object);
        var (builder, _, _) = CreateBuilder(services, _columns, AdminListLayout.Grid);
        var shape = await CreateListAsync(new AdminListContext<string> { Source = ["first", "second"] });

        await builder.BuildAsync(shape, TestContext.Current.CancellationToken);

        Assert.Equal([first, second], shape.GetProperty<IEnumerable<IShape>>("Rows"));
        Assert.False(shape.Properties.ContainsKey("Cells"));
        Assert.False(shape.Properties.ContainsKey("Headers"));
    }

    [Theory]
    [InlineData(5, 25, 3, 10, 21, 25)]
    [InlineData(2, null, 1, 10, 1, 2)]
    [InlineData(0, null, 1, 10, 0, 0)]
    public async Task BuildAsync_Toolbar_CountsItemsAndPager(int count, int? total, int page, int size, int start, int end)
    {
        var display = new Mock<IAdminListItemDisplayManager<string>>();
        display.Setup(manager => manager.BuildDisplayAsync(It.IsAny<string>(), It.IsAny<IUpdateModel>(), "SummaryAdmin")).ReturnsAsync(() => new Shape());
        using var services = CreateServices(display.Object);
        var (builder, _, _) = CreateBuilder(services, [], AdminListLayout.List);
        var pager = new Shape();
        pager.Properties["TotalItemCount"] = total;
        pager.Properties["Page"] = page;
        pager.Properties["PageSize"] = size;
        var bulkActions = new List<SelectListItem> { new("Delete", "Remove") };
        var filters = new Shape();
        var shape = await CreateListAsync(new AdminListContext<string>
        {
            Source = Enumerable.Repeat("item", count),
            Pager = total.HasValue ? pager : null,
            BulkActions = bulkActions,
            ToolbarActions = filters,
            ShowSelectAll = false,
        });

        await builder.BuildAsync(shape, TestContext.Current.CancellationToken);

        var toolbar = shape.GetProperty<IShape>("Toolbar");
        Assert.Equal(AdminListConstants.ToolbarShapeType, toolbar.Metadata.Type);
        Assert.Equal(count, toolbar.Properties["ItemsCount"]);
        Assert.Equal(total ?? count, toolbar.Properties["TotalItemCount"]);
        Assert.Equal(start, toolbar.Properties["StartIndex"]);
        Assert.Equal(end, toolbar.Properties["EndIndex"]);
        Assert.Equal(false, toolbar.Properties["ShowSelectAll"]);
        Assert.Same(bulkActions, toolbar.Properties["BulkActions"]);
        Assert.Same(filters, toolbar.Properties["Actions"]);
    }

    [Theory]
    [InlineData("Header")]
    [InlineData("Toolbar")]
    [InlineData("None")]
    public async Task BuildAsync_CustomOrDisabledToolbar_DoesNotBuildDefault(string mode)
    {
        using var services = CreateServices();
        var (builder, _, _) = CreateBuilder(services, [], AdminListLayout.List);
        var region = new Shape();
        var shape = await CreateListAsync(new AdminListContext
        {
            Header = mode == "Header" ? region : null,
            Toolbar = mode == "Toolbar" ? region : null,
            ShowToolbar = mode != "None",
        });

        await builder.BuildAsync(shape, TestContext.Current.CancellationToken);

        Assert.Same(mode == "Toolbar" ? region : null, shape.GetProperty<IShape>("Toolbar"));
    }

    private static readonly AdminListColumn[] _columns =
    [
        new() { Name = "Title", Position = "10" },
        AdminListColumns.Actions(new("Actions", "Actions")),
    ];

    private static ServiceProvider CreateServices(IAdminListItemDisplayManager<string> display = null)
        => new ServiceCollection()
            .AddSingleton(display ?? Mock.Of<IAdminListItemDisplayManager<string>>())
            .AddSingleton(Mock.Of<IUpdateModelAccessor>())
            .BuildServiceProvider();

    private static (AdminListBuilder Builder, HttpContext HttpContext, Mock<IAdminListColumnsBuilder> ColumnsBuilder) CreateBuilder(
        IServiceProvider services, IEnumerable<AdminListColumn> columns, AdminListLayout layout, bool allowSelection = false)
    {
        var http = new DefaultHttpContext();
        http.Request.QueryString = QueryString.Create("layout", layout.ToString());
        var accessor = new HttpContextAccessor { HttpContext = http };
        var options = Mock.Of<IOptionsMonitor<AdminListOptions>>(monitor => monitor.CurrentValue ==
            new AdminListOptions { DefaultLayout = layout, AllowUserSelection = allowSelection });
        var resolver = new AdminListLayoutResolver(options, accessor, new AdminListLayoutPreference(accessor));
        var columnsBuilder = new Mock<IAdminListColumnsBuilder>();
        columnsBuilder.Setup(value => value.BuildColumnsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<AdminListColumn>>(),
                It.IsAny<IReadOnlyDictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string name, IEnumerable<AdminListColumn> _, IReadOnlyDictionary<string, object> data, CancellationToken _) =>
                new AdminListColumnsContext(name, columns, data));
        return (new AdminListBuilder(resolver, columnsBuilder.Object, new AdminListTestShapeFactory(), options,
            NullLogger<AdminListBuilder>.Instance, new AdminListRowBuilder(services)), http, columnsBuilder);
    }

    private static async Task<IShape> CreateListAsync(AdminListContext data, string name = "Contents")
    {
        var context = new TagHelperContext("admin-list", [], new Dictionary<object, object>(), "test");
        var output = new TagHelperOutput("admin-list", [],
            (_, _) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));
        var display = new Mock<IDisplayHelper>();
        display.Setup(value => value.ShapeExecuteAsync(It.IsAny<IShape>())).ReturnsAsync(HtmlString.Empty);
        var tag = new AdminListTagHelper(display.Object, new AdminListTestShapeFactory()) { Name = name, Context = data };
        await tag.ProcessAsync(context, output);
        return (IShape)context.Items[typeof(AdminListTagHelper)];
    }
}

internal sealed class AdminListTestShapeFactory : IShapeFactory
{
    public dynamic New => this;

    public async ValueTask<IShape> CreateAsync(string shapeType, Func<ValueTask<IShape>> shapeFactory,
        Action<ShapeCreatingContext> creating, Action<ShapeCreatedContext> created)
    {
        var shape = await shapeFactory();
        shape.Metadata.Type = shapeType;
        created?.Invoke(new ShapeCreatedContext { Shape = shape, ShapeFactory = this, ShapeType = shapeType });
        return shape;
    }
}
