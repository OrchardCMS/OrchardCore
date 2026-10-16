using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;
using OrchardCore.Admin.Services;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.Tests.Modules.OrchardCore.Admin;

public class DefaultAdminListFactoryTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task CreateAsync_InvalidName_Throws(string name)
    {
        var (factory, _, _) = CreateFactory(_columns, AdminListLayout.Grid);

        var exception = await Assert.ThrowsAnyAsync<ArgumentException>(() =>
            factory.CreateAsync(name, new AdminListContext(), TestContext.Current.CancellationToken));

        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public async Task CreateAsync_NullContext_Throws()
    {
        var (factory, _, _) = CreateFactory(_columns, AdminListLayout.Grid);

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            factory.CreateAsync("Contents", null, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task CreateAsync_SharedContext_UsesEachRenderingName()
    {
        var (factory, _, builder) = CreateFactory(_columns, AdminListLayout.Grid);
        var context = new AdminListContext();
        context.Data["ContentTypes"] = new[] { "BlogPost" };

        var first = await factory.CreateAsync("First", context, TestContext.Current.CancellationToken);
        var second = await factory.CreateAsync("Second", context, TestContext.Current.CancellationToken);
        await factory.BuildAsync(first, TestContext.Current.CancellationToken);
        await factory.BuildAsync(second, TestContext.Current.CancellationToken);

        Assert.NotSame(first, second);
        Assert.Equal("First", first.GetProperty<string>("Name"));
        Assert.Equal("Second", second.GetProperty<string>("Name"));
        Assert.Same(context.Data, first.GetProperty<IReadOnlyDictionary<string, object>>("Data"));
        Assert.Same(context.Data, second.GetProperty<IReadOnlyDictionary<string, object>>("Data"));
        foreach (var name in new[] { "First", "Second" })
        {
            builder.Verify(value => value.BuildColumnsAsync(name, It.IsAny<IEnumerable<AdminListColumn>>(),
                context.Data, TestContext.Current.CancellationToken), Times.Once);
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public async Task BuildAsync_StandardShapeWithInvalidName_Throws(string name)
    {
        var (factory, _, _) = CreateFactory(_columns, AdminListLayout.Grid);
        var shape = new Shape();
        shape.Properties["Name"] = name;

        await Assert.ThrowsAnyAsync<ArgumentException>(() =>
            factory.BuildAsync(shape, TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData(AdminListLayout.List, true, "SummaryAdmin")]
    [InlineData(AdminListLayout.Grid, true, "SummaryAdminGrid")]
    [InlineData(AdminListLayout.Grid, false, "SummaryAdmin")]
    public async Task BuildAsync_SourceItems_BuildsSelectedDisplayTypeOnce(AdminListLayout layout, bool hasColumns, string displayType)
    {
        string[] items = ["item"];
        var row = new Shape();
        var rowBuilder = new Mock<IAdminListRowBuilder>();
        rowBuilder.Setup(builder => builder.BuildAsync(items, typeof(string), displayType, It.IsAny<CancellationToken>()))
            .ReturnsAsync([row]);
        var (factory, _, _) = CreateFactory(hasColumns ? _columns : [], layout, rowBuilder: rowBuilder.Object);
        var shape = await factory.CreateAsync("Items", new AdminListContext<string>
        {
            Source = items,
        }, TestContext.Current.CancellationToken);
        rowBuilder.VerifyNoOtherCalls();

        await factory.BuildAsync(shape, TestContext.Current.CancellationToken);
        await factory.BuildAsync(shape, TestContext.Current.CancellationToken);

        Assert.Same(row, Assert.Single(shape.GetProperty<IEnumerable<IShape>>("Rows")));
        Assert.Equal(displayType, shape.GetProperty<string>("DisplayType"));
        rowBuilder.Verify(builder => builder.BuildAsync(items, typeof(string), displayType, TestContext.Current.CancellationToken), Times.Once);
    }

    [Fact]
    public async Task BuildAsync_SourceAndCallback_RejectsAmbiguousInput()
    {
        var (factory, _, _) = CreateFactory(_columns, AdminListLayout.List);
        var shape = await factory.CreateAsync("Items", new AdminListContext
        {
            Source = Array.Empty<string>(),
            BuildRowsAsync = _ => Task.FromResult<IList<IShape>>([]),
        }, TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<InvalidOperationException>(() => factory.BuildAsync(shape, TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData(AdminListLayout.List, true, "SummaryAdmin")]
    [InlineData(AdminListLayout.Grid, true, "SummaryAdminGrid")]
    [InlineData(AdminListLayout.Grid, false, "SummaryAdmin")]
    public async Task BuildAsync_DeferredRows_UsesResolvedDisplayTypeOnce(AdminListLayout layout, bool hasColumns, string expectedDisplayType)
    {
        var (factory, _, _) = CreateFactory(hasColumns ? _columns : [], layout);
        var calls = 0;
        var row = new Shape();
        var shape = await factory.CreateAsync("Contents", new AdminListContext
        {
            BuildRowsAsync = displayType =>
            {
                Assert.Equal(expectedDisplayType, displayType);
                calls++;
                return Task.FromResult<IList<IShape>>([row]);
            },
        }, TestContext.Current.CancellationToken);
        Assert.Equal(0, calls);

        await factory.BuildAsync(shape, TestContext.Current.CancellationToken);
        await factory.BuildAsync(shape, TestContext.Current.CancellationToken);

        Assert.Equal(1, calls);
        Assert.Equal(expectedDisplayType, shape.Properties["DisplayType"]);
        Assert.Same(row, Assert.Single(shape.GetProperty<IEnumerable<IShape>>("Rows")));
        Assert.Equal(1, shape.GetProperty<IShape>("Toolbar").Properties["ItemsCount"]);
    }

    private static readonly AdminListColumn[] _columns =
    [
        new() { Name = "Title", Position = "10", Zones = ["Title"] },
        new() { Name = "Actions", Position = "end", Zones = ["Actions"] },
    ];

    [Fact]
    public async Task BuildAsync_UndefinedLayout_Throws()
    {
        var (factory, _, _) = CreateFactory(_columns, AdminListLayout.List);
        var shape = await factory.CreateAsync("Contents", new AdminListContext
        {
            Layout = (AdminListLayout)42,
        }, TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => factory.BuildAsync(shape, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task CreateAsync_ViewConfiguresColumnsBeforeDisplay_UsesViewProperties()
    {
        var (factory, _, builder) = CreateFactory(_columns, AdminListLayout.Grid);
        var shape = await factory.CreateAsync("Contents", new AdminListContext(), TestContext.Current.CancellationToken);
        var columns = new[] { new AdminListColumn { Name = "ViewColumn" } };

        builder.Verify(b => b.BuildColumnsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<AdminListColumn>>(),
            It.IsAny<IReadOnlyDictionary<string, object>>(), It.IsAny<CancellationToken>()), Times.Never);

        shape.Properties["Columns"] = columns;
        shape.Properties["ItemCssClass"] = "from-view";
        await factory.BuildAsync(shape, TestContext.Current.CancellationToken);
        await factory.BuildAsync(shape, TestContext.Current.CancellationToken);

        builder.Verify(b => b.BuildColumnsAsync("Contents", columns,
            It.IsAny<IReadOnlyDictionary<string, object>>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal("from-view", shape.Properties["ItemCssClass"]);
    }

    [Fact]
    public async Task BuildAsync_StandardShapeWithoutContext_ResolvesLayoutAndToolbar()
    {
        var (factory, _, _) = CreateFactory(_columns, AdminListLayout.Grid);
        var shape = new Shape();
        shape.Metadata.Type = AdminListConstants.ShapeType;
        shape.Properties["Name"] = "Contents";
        shape.Properties["Rows"] = new IShape[] { new Shape() };
        shape.Properties["Columns"] = _columns;

        await factory.BuildAsync(shape, TestContext.Current.CancellationToken);

        Assert.Equal(AdminListLayout.Grid, shape.Properties["Layout"]);
        Assert.Single(shape.GetProperty<IEnumerable<IShape>>("Rows"));
        Assert.Equal(1, shape.GetProperty<IShape>("Toolbar").Properties["ItemsCount"]);
    }

    [Fact]
    public async Task CreateAsync_ListWithColumns_GetsItsColumnsAndTheResolvedLayout()
    {
        var (factory, _, _) = CreateFactory(_columns, AdminListLayout.Grid);
        var row = new Shape();
        var header = new Shape();
        var pager = new Shape();
        var emptyMessage = new HtmlString("Nothing here!");

        var shape = await factory.CreateAsync("Contents", new AdminListContext
        {
            Rows = [row],
            Header = header,
            Pager = pager,
            ItemCssClass = "list-group-item",
            EmptyMessage = emptyMessage,
        }, TestContext.Current.CancellationToken);
        await factory.BuildAsync(shape, TestContext.Current.CancellationToken);

        Assert.Equal(AdminListConstants.ShapeType, shape.Metadata.Type);
        Assert.Equal("Contents", shape.Properties["Name"]);
        Assert.Equal(AdminListLayout.Grid, shape.Properties["Layout"]);
        Assert.Equal(["Title", "Actions"], ((IEnumerable<AdminListColumn>)shape.Properties["Columns"]).Select(column => column.Name));
        Assert.Same(row, Assert.Single((IEnumerable<object>)shape.Properties["Rows"]));
        Assert.Same(header, shape.Properties["Header"]);
        Assert.Same(pager, shape.Properties["Pager"]);
        Assert.Equal("list-group-item", shape.Properties["ItemCssClass"]);
        Assert.Same(emptyMessage, shape.Properties["EmptyMessage"]);
        Assert.Equal(true, shape.Properties["ShowLayoutSelector"]);

        // The Grid layout renders the zones of each row directly.
        Assert.Single(shape.GetProperty<IEnumerable<IShape>>("Rows"));

        // The parts the page left out are not set: a shape reads a property it does not have as null.
        Assert.Null(shape.GetProperty("Toolbar"));
        Assert.Null(shape.GetProperty("Search"));
        Assert.Null(shape.GetProperty("RowsAttributes"));

    }

    [Fact]
    public async Task CreateAsync_LayoutSetByThePage_IsKept()
    {
        var (factory, httpContext, _) = CreateFactory(_columns, AdminListLayout.Grid, allowUserSelection: true);

        // e.g. the items of a list part ordered by dragging them, which the page renders with the List layout.
        var shape = await factory.CreateAsync("ListPartContents", new AdminListContext
        {
            Layout = AdminListLayout.List,
        }, TestContext.Current.CancellationToken);
        await factory.BuildAsync(shape, TestContext.Current.CancellationToken);

        Assert.Equal(AdminListLayout.List, shape.Properties["Layout"]);
        Assert.False(httpContext.Response.Headers.ContainsKey("Set-Cookie"));
    }

    [Fact]
    public async Task CreateAsync_ListWithoutColumns_IsRenderedAsAListWithoutSelector()
    {
        var (factory, httpContext, _) = CreateFactory([], AdminListLayout.Grid, allowUserSelection: true);

        var shape = await factory.CreateAsync("Unknown", new AdminListContext(), TestContext.Current.CancellationToken);
        await factory.BuildAsync(shape, TestContext.Current.CancellationToken);

        // Without columns the rows can only be rendered whole, so there is no other layout to offer, and the
        // resolver is not asked: it would remember a layout a user asked for in the query string.
        Assert.Equal(AdminListLayout.List, shape.Properties["Layout"]);
        Assert.Equal(false, shape.Properties["ShowLayoutSelector"]);
        Assert.False(httpContext.Response.Headers.ContainsKey("Set-Cookie"));
    }

    [Fact]
    public async Task CreateAsync_DefaultLayoutOfAListWithoutColumns_IsRenderedAsAList()
    {
        var (factory, _, _) = CreateFactory([], AdminListLayout.Grid);

        var shape = await factory.CreateAsync("Unknown", new AdminListContext(), TestContext.Current.CancellationToken);
        await factory.BuildAsync(shape, TestContext.Current.CancellationToken);

        Assert.Equal(AdminListLayout.List, shape.Properties["Layout"]);
        Assert.Equal(false, shape.Properties["ShowLayoutSelector"]);
    }

    [Fact]
    public async Task CreateAsync_ListLayoutWithoutUserSelection_DoesNotBuildTheColumns()
    {
        var (factory, _, columnsBuilder) = CreateFactory(_columns, AdminListLayout.List);

        var shape = await factory.CreateAsync("Contents", new AdminListContext(), TestContext.Current.CancellationToken);
        await factory.BuildAsync(shape, TestContext.Current.CancellationToken);

        // The List layout renders the rows whole, and there is no other layout to offer.
        Assert.Equal(AdminListLayout.List, shape.Properties["Layout"]);
        Assert.Empty((IEnumerable<AdminListColumn>)shape.Properties["Columns"]);
        Assert.Equal(false, shape.Properties["ShowLayoutSelector"]);
        columnsBuilder.Verify(b => b.BuildColumnsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<AdminListColumn>>(), It.IsAny<IReadOnlyDictionary<string, object>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ListLayoutSetByThePageWithoutSelector_DoesNotBuildTheColumns()
    {
        var (factory, _, columnsBuilder) = CreateFactory(_columns, AdminListLayout.Grid, allowUserSelection: true);

        var shape = await factory.CreateAsync("ListPartContents", new AdminListContext
        {
            Layout = AdminListLayout.List,
            ShowLayoutSelector = false,
        }, TestContext.Current.CancellationToken);
        await factory.BuildAsync(shape, TestContext.Current.CancellationToken);

        Assert.Empty((IEnumerable<AdminListColumn>)shape.Properties["Columns"]);
        columnsBuilder.Verify(b => b.BuildColumnsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<AdminListColumn>>(), It.IsAny<IReadOnlyDictionary<string, object>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ListLayoutOfferingTheOtherLayouts_BuildsTheColumns()
    {
        var (factory, httpContext, _) = CreateFactory(_columns, AdminListLayout.List, allowUserSelection: true);

        var shape = await factory.CreateAsync("Contents", new AdminListContext(), TestContext.Current.CancellationToken);
        await factory.BuildAsync(shape, TestContext.Current.CancellationToken);

        // The selector only offers the layouts with columns to a list that has some.
        Assert.Equal(AdminListLayout.List, shape.Properties["Layout"]);
        Assert.Equal(["Title", "Actions"], ((IEnumerable<AdminListColumn>)shape.Properties["Columns"]).Select(column => column.Name));
        Assert.Equal(true, shape.Properties["ShowLayoutSelector"]);
        Assert.Contains("Contents%3AList", httpContext.Response.Headers.SetCookie.ToString());
    }

    [Fact]
    public async Task BuildAsync_ListLayout_DoesNotCreateCellCopies()
    {
        var (factory, _, _) = CreateFactory(_columns, AdminListLayout.List, allowUserSelection: true);

        var shape = await factory.CreateAsync("Contents", new AdminListContext
        {
            Rows = [new Shape()],
        }, TestContext.Current.CancellationToken);
        await factory.BuildAsync(shape, TestContext.Current.CancellationToken);

        // The List layout renders the rows whole.
        Assert.False(shape.Properties.ContainsKey("Headers"));
        Assert.False(shape.Properties.ContainsKey("Cells"));
    }

    [Fact]
    public async Task BuildAsync_GridLayout_PreservesRowOrder()
    {
        var (factory, _, _) = CreateFactory(_columns, AdminListLayout.Grid);
        var first = new Shape();
        var second = new Shape();

        var shape = await factory.CreateAsync("Contents", new AdminListContext
        {
            Rows = [first, second],
        }, TestContext.Current.CancellationToken);
        await factory.BuildAsync(shape, TestContext.Current.CancellationToken);

        Assert.Equal([first, second], (IEnumerable<IShape>)shape.Properties["Rows"]);
    }

    [Fact]
    public async Task CreateAsync_PageTurnsTheSelectorOff_TheShapeSaysSo()
    {
        var (factory, _, _) = CreateFactory(_columns, AdminListLayout.List);

        var shape = await factory.CreateAsync("Features", new AdminListContext
        {
            ShowLayoutSelector = false,
        }, TestContext.Current.CancellationToken);
        await factory.BuildAsync(shape, TestContext.Current.CancellationToken);

        Assert.Equal(false, shape.Properties["ShowLayoutSelector"]);
    }

    [Fact]
    public async Task CreateAsync_DataOfThePage_ReachesTheColumnsBuilder()
    {
        var (factory, _, columnsBuilder) = CreateFactory(_columns, AdminListLayout.Grid);
        var context = new AdminListContext();
        context.Data["ContentTypes"] = new[] { "BlogPost" };

        var shape = await factory.CreateAsync("Contents", context, TestContext.Current.CancellationToken);
        await factory.BuildAsync(shape, TestContext.Current.CancellationToken);

        columnsBuilder.Verify(b => b.BuildColumnsAsync(
            "Contents",
            It.IsAny<IEnumerable<AdminListColumn>>(),
            It.Is<IReadOnlyDictionary<string, object>>(data => ((string[])data["ContentTypes"]).Single() == "BlogPost"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ListWithoutHeaderOrToolbar_GetsTheToolbarCountingThePageOfItems()
    {
        var (factory, _, _) = CreateFactory(_columns, AdminListLayout.List);
        var bulkActions = new List<SelectListItem> { new("Delete", "Remove") };
        var filters = new Shape();

        // The third page of 10 items out of 25 holds the last 5.
        var pager = new Shape();
        pager.Properties["Page"] = 3;
        pager.Properties["PageSize"] = 10;
        pager.Properties["TotalItemCount"] = 25;

        var shape = await factory.CreateAsync("Queries", new AdminListContext
        {
            Rows = [.. Enumerable.Range(0, 5).Select(_ => (IShape)new Shape())],
            Pager = pager,
            BulkActions = bulkActions,
            ToolbarActions = filters,
        }, TestContext.Current.CancellationToken);
        await factory.BuildAsync(shape, TestContext.Current.CancellationToken);

        var toolbar = Assert.IsAssignableFrom<IShape>(shape.Properties["Toolbar"]);

        Assert.Equal(AdminListConstants.ToolbarShapeType, toolbar.Metadata.Type);
        Assert.Equal(5, toolbar.Properties["ItemsCount"]);
        Assert.Equal(25, toolbar.Properties["TotalItemCount"]);
        Assert.Equal(21, toolbar.Properties["StartIndex"]);
        Assert.Equal(25, toolbar.Properties["EndIndex"]);
        Assert.Equal(true, toolbar.Properties["ShowSelectAll"]);
        Assert.Same(bulkActions, toolbar.Properties["BulkActions"]);
        Assert.Same(filters, toolbar.Properties["Actions"]);
    }

    [Theory]
    [InlineData(2, 1, 2)]
    [InlineData(0, 0, 0)]
    public async Task CreateAsync_ListWithoutPager_CountsEveryRow(int rowCount, int startIndex, int endIndex)
    {
        var (factory, _, _) = CreateFactory(_columns, AdminListLayout.List);

        var shape = await factory.CreateAsync("Roles", new AdminListContext
        {
            Rows = Enumerable.Range(0, rowCount).Select(_ => (IShape)new Shape()).ToList(),
            ShowSelectAll = false,
        }, TestContext.Current.CancellationToken);
        await factory.BuildAsync(shape, TestContext.Current.CancellationToken);

        var toolbar = Assert.IsAssignableFrom<IShape>(shape.Properties["Toolbar"]);

        Assert.Equal(rowCount, toolbar.Properties["ItemsCount"]);
        Assert.Equal(rowCount, toolbar.Properties["TotalItemCount"]);
        Assert.Equal(startIndex, toolbar.Properties["StartIndex"]);
        Assert.Equal(endIndex, toolbar.Properties["EndIndex"]);
        Assert.Equal(false, toolbar.Properties["ShowSelectAll"]);

        // A shape reads a property it does not have as null, which the toolbar renders as nothing.
        Assert.Null(toolbar.GetProperty("BulkActions"));
        Assert.Null(toolbar.GetProperty("Actions"));
    }

    [Fact]
    public async Task CreateAsync_PageWithHeaderOwnToolbarOrNone_GetsNoGenericToolbar()
    {
        var (factory, _, _) = CreateFactory(_columns, AdminListLayout.List);
        var ownToolbar = new Shape();

        var withHeader = await factory.CreateAsync("Contents", new AdminListContext { Header = new Shape() }, TestContext.Current.CancellationToken);
        await factory.BuildAsync(withHeader, TestContext.Current.CancellationToken);
        var withOwnToolbar = await factory.CreateAsync("RateLimits", new AdminListContext { Toolbar = ownToolbar }, TestContext.Current.CancellationToken);
        await factory.BuildAsync(withOwnToolbar, TestContext.Current.CancellationToken);
        var withoutToolbar = await factory.CreateAsync("Recipes", new AdminListContext { ShowToolbar = false }, TestContext.Current.CancellationToken);
        await factory.BuildAsync(withoutToolbar, TestContext.Current.CancellationToken);

        Assert.Null(withHeader.GetProperty("Toolbar"));
        Assert.Same(ownToolbar, withOwnToolbar.Properties["Toolbar"]);
        Assert.Null(withoutToolbar.GetProperty("Toolbar"));
    }

    private static (DefaultAdminListFactory Factory, HttpContext HttpContext, Mock<IAdminListColumnsBuilder> ColumnsBuilder) CreateFactory(
        IList<AdminListColumn> columns,
        AdminListLayout layout,
        bool allowUserSelection = false,
        IAdminListRowBuilder rowBuilder = null)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Request.QueryString = QueryString.Create("layout", layout.ToString());
        var accessor = new HttpContextAccessor { HttpContext = httpContext };
        var options = Mock.Of<IOptionsMonitor<AdminListOptions>>(monitor => monitor.CurrentValue ==
            new AdminListOptions { DefaultLayout = layout, AllowUserSelection = allowUserSelection });
        var layoutResolver = new AdminListLayoutResolver(options, accessor, new AdminListLayoutPreference(accessor));

        var columnsBuilder = new Mock<IAdminListColumnsBuilder>();
        columnsBuilder
            .Setup(b => b.BuildColumnsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<AdminListColumn>>(), It.IsAny<IReadOnlyDictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string listName, IEnumerable<AdminListColumn> _, IReadOnlyDictionary<string, object> data, CancellationToken _) =>
            {
                var context = new AdminListColumnsContext(listName, columns, data);

                return context;
            });

        var factory = new DefaultAdminListFactory(
            layoutResolver,
            columnsBuilder.Object,
            new TestShapeFactory(),
            options,
            NullLogger<DefaultAdminListFactory>.Instance,
            rowBuilder ?? Mock.Of<IAdminListRowBuilder>());

        return (factory, httpContext, columnsBuilder);
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
