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
