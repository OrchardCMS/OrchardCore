using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.Admin;
using OrchardCore.Admin.Models;
using OrchardCore.Admin.Services;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.Shapes;

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

        var shape = await factory.CreateAsync(new AdminListContext("Contents")
        {
            Rows = [row],
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
        var shape = await factory.CreateAsync(new AdminListContext("ListPartContents")
        {
            Layout = AdminListConstants.List,
        }, TestContext.Current.CancellationToken);

        Assert.Equal(AdminListConstants.List, shape.Properties["Layout"]);
        layoutResolver.Verify(r => r.GetLayoutAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ListWithoutColumns_IsRenderedAsAListWithoutSelector()
    {
        var (factory, layoutResolver, _) = CreateFactory([], AdminListConstants.Grid);

        var shape = await factory.CreateAsync(new AdminListContext("Unknown"), TestContext.Current.CancellationToken);

        // Without columns the rows can only be rendered whole, so there is no other layout to offer.
        Assert.Equal(AdminListConstants.List, shape.Properties["Layout"]);
        Assert.Equal(false, shape.Properties["ShowLayoutSelector"]);
        layoutResolver.Verify(r => r.GetLayoutAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_PageTurnsTheSelectorOff_TheShapeSaysSo()
    {
        var (factory, _, _) = CreateFactory(_columns, AdminListConstants.List);

        var shape = await factory.CreateAsync(new AdminListContext("Features")
        {
            ShowLayoutSelector = false,
        }, TestContext.Current.CancellationToken);

        Assert.Equal(false, shape.Properties["ShowLayoutSelector"]);
    }

    [Fact]
    public async Task CreateAsync_DataOfThePage_ReachesTheColumnsBuilder()
    {
        var (factory, _, columnsBuilder) = CreateFactory(_columns, AdminListConstants.List);
        var context = new AdminListContext("Contents");
        context.Data["ContentTypes"] = new[] { "BlogPost" };

        await factory.CreateAsync(context, TestContext.Current.CancellationToken);

        columnsBuilder.Verify(b => b.BuildAsync(
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

        var shape = await factory.CreateAsync(new AdminListContext("Queries")
        {
            Rows = [new Shape(), new Shape(), new Shape(), new Shape(), new Shape()],
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

        var shape = await factory.CreateAsync(new AdminListContext("Roles")
        {
            Rows = Enumerable.Range(0, rowCount).Select(_ => (IShape)new Shape()).ToList(),
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

        var withHeader = await factory.CreateAsync(new AdminListContext("Contents") { Header = new Shape() }, TestContext.Current.CancellationToken);
        var withOwnToolbar = await factory.CreateAsync(new AdminListContext("RateLimits") { Toolbar = ownToolbar }, TestContext.Current.CancellationToken);
        var withoutToolbar = await factory.CreateAsync(new AdminListContext("Recipes") { ShowToolbar = false }, TestContext.Current.CancellationToken);

        Assert.False(withHeader.Properties.ContainsKey("Toolbar"));
        Assert.Same(ownToolbar, withOwnToolbar.Properties["Toolbar"]);
        Assert.False(withoutToolbar.Properties.ContainsKey("Toolbar"));
    }

    [Fact]
    public async Task CreateAsync_RowsEnumerableOnce_AreCountedAndRenderedFromOneEnumeration()
    {
        var (factory, _, _) = CreateFactory(_columns, AdminListConstants.List);
        var enumerations = 0;

        IEnumerable<IShape> Rows()
        {
            enumerations++;

            yield return new Shape();
            yield return new Shape();
        }

        var shape = await factory.CreateAsync(new AdminListContext("Queries") { Rows = Rows() }, TestContext.Current.CancellationToken);

        var toolbar = Assert.IsAssignableFrom<IShape>(shape.Properties["Toolbar"]);

        Assert.Equal(2, toolbar.Properties["ItemsCount"]);
        Assert.Equal(2, ((IEnumerable<IShape>)shape.Properties["Rows"]).Count());
        Assert.Equal(1, enumerations);
    }

    private static (DefaultAdminListFactory Factory, Mock<IAdminListLayoutResolver> LayoutResolver, Mock<IAdminListColumnsBuilder> ColumnsBuilder) CreateFactory(
        IList<AdminListColumn> columns,
        string layout)
    {
        var layoutResolver = new Mock<IAdminListLayoutResolver>();
        layoutResolver
            .Setup(r => r.GetLayoutAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(layout);

        var columnsBuilder = new Mock<IAdminListColumnsBuilder>();
        columnsBuilder
            .Setup(b => b.BuildAsync(It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => columns.ToList());

        var factory = new DefaultAdminListFactory(
            layoutResolver.Object,
            columnsBuilder.Object,
            new TestShapeFactory(),
            NullLogger<DefaultAdminListFactory>.Instance);

        return (factory, layoutResolver, columnsBuilder);
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
