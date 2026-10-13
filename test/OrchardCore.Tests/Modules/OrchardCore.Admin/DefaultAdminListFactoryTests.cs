using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;
using OrchardCore.Admin.Services;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Zones;

namespace OrchardCore.Tests.Modules.OrchardCore.Admin;

public class DefaultAdminListFactoryTests
{
    private static readonly AdminListColumn[] _columns =
    [
        new() { Name = "Title", Position = "10", Zones = ["Title"] },
        new() { Name = "Actions", Position = "end", Zones = ["Actions"] },
    ];

    [Fact]
    public async Task CreateAsync_ListWithColumns_GetsItsColumnsAndTheResolvedLayout()
    {
        var (factory, layoutResolver, _) = CreateFactory(_columns, AdminListConstants.Grid);
        var row = new Shape();
        var header = new Shape();
        var pager = new Shape();
        var emptyMessage = new HtmlString("Nothing here!");

        var shape = await factory.CreateAsync(new AdminListContext<object>("Contents")
        {
            Rows = [new(new object(), row)],
            Header = header,
            Pager = pager,
            ItemCssClass = "list-group-item",
            EmptyMessage = emptyMessage,
        }, TestContext.Current.CancellationToken);

        Assert.Equal(AdminListConstants.ShapeType, shape.Metadata.Type);
        Assert.Equal("Contents", shape.Properties["Name"]);
        Assert.Equal(AdminListConstants.Grid, shape.Properties["Layout"]);
        Assert.Equal(["Title", "Actions"], ((IEnumerable<AdminListColumn>)shape.Properties["Columns"]).Select(column => column.Name));
        Assert.Same(row, Assert.Single((IEnumerable<object>)shape.Properties["Rows"]));
        Assert.Same(header, shape.Properties["Header"]);
        Assert.Same(pager, shape.Properties["Pager"]);
        Assert.Equal("list-group-item", shape.Properties["ItemCssClass"]);
        Assert.Same(emptyMessage, shape.Properties["EmptyMessage"]);
        Assert.Equal(true, shape.Properties["ShowLayoutSelector"]);

        // The Grid layout renders the headers and the cells the column drivers built, one per row.
        Assert.IsAssignableFrom<IZoneHolding>(shape.Properties["Headers"]);
        Assert.Single((IList<IZoneHolding>)shape.Properties["Cells"]);

        // The parts the page left out are not set: a shape reads a property it does not have as null.
        Assert.False(shape.Properties.ContainsKey("Toolbar"));
        Assert.False(shape.Properties.ContainsKey("Search"));
        Assert.False(shape.Properties.ContainsKey("RowsAttributes"));

        layoutResolver.Verify(r => r.GetLayoutAsync("Contents", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_LayoutSetByThePage_IsKept()
    {
        var (factory, layoutResolver, _) = CreateFactory(_columns, AdminListConstants.Grid);

        // e.g. the items of a list part ordered by dragging them, which the page renders with the List layout.
        var shape = await factory.CreateAsync(new AdminListContext<object>("ListPartContents")
        {
            Layout = AdminListConstants.List,
        }, TestContext.Current.CancellationToken);

        Assert.Equal(AdminListConstants.List, shape.Properties["Layout"]);
        layoutResolver.Verify(r => r.GetLayoutAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ListWithoutColumns_IsRenderedAsAListWithoutSelector()
    {
        var (factory, layoutResolver, _) = CreateFactory([], AdminListConstants.Grid, allowUserSelection: true);

        var shape = await factory.CreateAsync(new AdminListContext<object>("Unknown"), TestContext.Current.CancellationToken);

        // Without columns the rows can only be rendered whole, so there is no other layout to offer, and the
        // resolver is not asked: it would remember a layout a user asked for in the query string.
        Assert.Equal(AdminListConstants.List, shape.Properties["Layout"]);
        Assert.Equal(false, shape.Properties["ShowLayoutSelector"]);
        layoutResolver.Verify(r => r.GetLayoutAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_DefaultLayoutOfAListWithoutColumns_IsRenderedAsAList()
    {
        var (factory, _, _) = CreateFactory([], AdminListConstants.Grid);

        var shape = await factory.CreateAsync(new AdminListContext<object>("Unknown"), TestContext.Current.CancellationToken);

        Assert.Equal(AdminListConstants.List, shape.Properties["Layout"]);
        Assert.Equal(false, shape.Properties["ShowLayoutSelector"]);
    }

    [Fact]
    public async Task CreateAsync_ListLayoutWithoutUserSelection_DoesNotBuildTheColumns()
    {
        var (factory, _, columnsBuilder) = CreateFactory(_columns, AdminListConstants.List);

        var shape = await factory.CreateAsync(new AdminListContext<object>("Contents"), TestContext.Current.CancellationToken);

        // The List layout renders the rows whole, and there is no other layout to offer.
        Assert.Equal(AdminListConstants.List, shape.Properties["Layout"]);
        Assert.Empty((IEnumerable<AdminListColumn>)shape.Properties["Columns"]);
        Assert.Equal(false, shape.Properties["ShowLayoutSelector"]);
        columnsBuilder.Verify(b => b.BuildColumnsAsync<object>(It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, object>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ListLayoutSetByThePageWithoutSelector_DoesNotBuildTheColumns()
    {
        var (factory, _, columnsBuilder) = CreateFactory(_columns, AdminListConstants.Grid, allowUserSelection: true);

        var shape = await factory.CreateAsync(new AdminListContext<object>("ListPartContents")
        {
            Layout = AdminListConstants.List,
            ShowLayoutSelector = false,
        }, TestContext.Current.CancellationToken);

        Assert.Empty((IEnumerable<AdminListColumn>)shape.Properties["Columns"]);
        columnsBuilder.Verify(b => b.BuildColumnsAsync<object>(It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, object>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ListLayoutOfferingTheOtherLayouts_BuildsTheColumns()
    {
        var (factory, layoutResolver, _) = CreateFactory(_columns, AdminListConstants.List, allowUserSelection: true);

        var shape = await factory.CreateAsync(new AdminListContext<object>("Contents"), TestContext.Current.CancellationToken);

        // The selector only offers the layouts with columns to a list that has some.
        Assert.Equal(AdminListConstants.List, shape.Properties["Layout"]);
        Assert.Equal(["Title", "Actions"], ((IEnumerable<AdminListColumn>)shape.Properties["Columns"]).Select(column => column.Name));
        Assert.Equal(true, shape.Properties["ShowLayoutSelector"]);
        layoutResolver.Verify(r => r.GetLayoutAsync("Contents", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ListLayout_DoesNotBuildTheCells()
    {
        var (factory, _, columnsBuilder) = CreateFactory(_columns, AdminListConstants.List, allowUserSelection: true);

        var shape = await factory.CreateAsync(new AdminListContext<object>("Contents")
        {
            Rows = [new(new object(), new Shape())],
        }, TestContext.Current.CancellationToken);

        // The List layout renders the rows whole, so the cells the column drivers would build are not rendered.
        Assert.False(shape.Properties.ContainsKey("Headers"));
        Assert.False(shape.Properties.ContainsKey("Cells"));
        columnsBuilder.Verify(b => b.BuildCellsAsync(It.IsAny<AdminListColumnsContext>(), It.IsAny<IEnumerable<AdminListRow<object>>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_GridLayout_BuildsTheCellsOfTheRowsInOrder()
    {
        var (factory, _, columnsBuilder) = CreateFactory(_columns, AdminListConstants.Grid);
        var first = new AdminListRow<object>("First", new Shape());
        var second = new AdminListRow<object>("Second", new Shape());

        var shape = await factory.CreateAsync(new AdminListContext<object>("Contents")
        {
            Rows = [first, second],
        }, TestContext.Current.CancellationToken);

        Assert.Equal([first.Shape, second.Shape], (IEnumerable<IShape>)shape.Properties["Rows"]);
        Assert.Equal(2, ((IList<IZoneHolding>)shape.Properties["Cells"]).Count);
        columnsBuilder.Verify(b => b.BuildCellsAsync(
            It.IsAny<AdminListColumnsContext>(),
            It.Is<IEnumerable<AdminListRow<object>>>(rows => rows.SequenceEqual(new[] { first, second })),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_PageTurnsTheSelectorOff_TheShapeSaysSo()
    {
        var (factory, _, _) = CreateFactory(_columns, AdminListConstants.List);

        var shape = await factory.CreateAsync(new AdminListContext<object>("Features")
        {
            ShowLayoutSelector = false,
        }, TestContext.Current.CancellationToken);

        Assert.Equal(false, shape.Properties["ShowLayoutSelector"]);
    }

    [Fact]
    public async Task CreateAsync_DataOfThePage_ReachesTheColumnsBuilder()
    {
        var (factory, _, columnsBuilder) = CreateFactory(_columns, AdminListConstants.Grid);
        var context = new AdminListContext<object>("Contents");
        context.Data["ContentTypes"] = new[] { "BlogPost" };

        await factory.CreateAsync(context, TestContext.Current.CancellationToken);

        columnsBuilder.Verify(b => b.BuildColumnsAsync<object>(
            "Contents",
            It.Is<IReadOnlyDictionary<string, object>>(data => ((string[])data["ContentTypes"]).Single() == "BlogPost"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ListWithoutHeaderOrToolbar_GetsTheToolbarCountingThePageOfItems()
    {
        var (factory, _, _) = CreateFactory(_columns, AdminListConstants.List);
        var bulkActions = new List<SelectListItem> { new("Delete", "Remove") };
        var filters = new Shape();

        // The third page of 10 items out of 25 holds the last 5.
        var pager = new Shape();
        pager.Properties["Page"] = 3;
        pager.Properties["PageSize"] = 10;
        pager.Properties["TotalItemCount"] = 25;

        var shape = await factory.CreateAsync(new AdminListContext<object>("Queries")
        {
            Rows = [.. Enumerable.Range(0, 5).Select(_ => new AdminListRow<object>(new object(), new Shape()))],
            Pager = pager,
            BulkActions = bulkActions,
            ToolbarActions = filters,
        }, TestContext.Current.CancellationToken);

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
        var (factory, _, _) = CreateFactory(_columns, AdminListConstants.List);

        var shape = await factory.CreateAsync(new AdminListContext<object>("Roles")
        {
            Rows = Enumerable.Range(0, rowCount).Select(_ => new AdminListRow<object>(new object(), new Shape())).ToList(),
            ShowSelectAll = false,
        }, TestContext.Current.CancellationToken);

        var toolbar = Assert.IsAssignableFrom<IShape>(shape.Properties["Toolbar"]);

        Assert.Equal(rowCount, toolbar.Properties["ItemsCount"]);
        Assert.Equal(rowCount, toolbar.Properties["TotalItemCount"]);
        Assert.Equal(startIndex, toolbar.Properties["StartIndex"]);
        Assert.Equal(endIndex, toolbar.Properties["EndIndex"]);
        Assert.Equal(false, toolbar.Properties["ShowSelectAll"]);

        // A shape reads a property it does not have as null, which the toolbar renders as nothing.
        Assert.False(toolbar.Properties.ContainsKey("BulkActions"));
        Assert.False(toolbar.Properties.ContainsKey("Actions"));
    }

    [Fact]
    public async Task CreateAsync_PageWithHeaderOwnToolbarOrNone_GetsNoGenericToolbar()
    {
        var (factory, _, _) = CreateFactory(_columns, AdminListConstants.List);
        var ownToolbar = new Shape();

        var withHeader = await factory.CreateAsync(new AdminListContext<object>("Contents") { Header = new Shape() }, TestContext.Current.CancellationToken);
        var withOwnToolbar = await factory.CreateAsync(new AdminListContext<object>("RateLimits") { Toolbar = ownToolbar }, TestContext.Current.CancellationToken);
        var withoutToolbar = await factory.CreateAsync(new AdminListContext<object>("Recipes") { ShowToolbar = false }, TestContext.Current.CancellationToken);

        Assert.False(withHeader.Properties.ContainsKey("Toolbar"));
        Assert.Same(ownToolbar, withOwnToolbar.Properties["Toolbar"]);
        Assert.False(withoutToolbar.Properties.ContainsKey("Toolbar"));
    }

    private static (DefaultAdminListFactory Factory, Mock<IAdminListLayoutResolver> LayoutResolver, Mock<IAdminListColumnsBuilder> ColumnsBuilder) CreateFactory(
        IList<AdminListColumn> columns,
        string layout,
        bool allowUserSelection = false)
    {
        var layoutResolver = new Mock<IAdminListLayoutResolver>();
        layoutResolver
            .Setup(r => r.GetLayoutAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(layout);

        var columnsBuilder = new Mock<IAdminListColumnsBuilder>();
        columnsBuilder
            .Setup(b => b.BuildColumnsAsync<object>(It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string listName, IReadOnlyDictionary<string, object> data, CancellationToken _) =>
            {
                var context = new AdminListColumnsContext(listName, data, CreateZoneHolding(), new TestShapeFactory(), null, null);

                foreach (var column in columns)
                {
                    context.Columns.Add(column);
                }

                return context;
            });
        columnsBuilder
            .Setup(b => b.BuildCellsAsync(It.IsAny<AdminListColumnsContext>(), It.IsAny<IEnumerable<AdminListRow<object>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AdminListColumnsContext _, IEnumerable<AdminListRow<object>> rows, CancellationToken _) =>
                rows.Select(_ => (IZoneHolding)CreateZoneHolding()).ToList());

        var factory = new DefaultAdminListFactory(
            layoutResolver.Object,
            columnsBuilder.Object,
            new TestShapeFactory(),
            Mock.Of<IOptionsMonitor<AdminListOptions>>(m => m.CurrentValue == new AdminListOptions { AllowUserSelection = allowUserSelection }),
            NullLogger<DefaultAdminListFactory>.Instance);

        return (factory, layoutResolver, columnsBuilder);
    }

    private static ZoneHolding CreateZoneHolding()
        => new(() => ValueTask.FromResult<IShape>(new Shape()));

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
