using System.Collections;
using OrchardCore.Admin;
using OrchardCore.Admin.Services;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.Contents.Services;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Implementation;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Zones;
using OrchardCore.Lists.Models;
using OrchardCore.Lists.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.Admin;

public class AdminListRowBuilderTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    public async Task BuildAsync_TypedItems_InfersTypeIncludingEmptyCollections(int count)
    {
        var items = Enumerable.Range(0, count).Select(index => index.ToString()).ToArray();
        var updater = Mock.Of<IUpdateModel>();
        var display = new Mock<IAdminListItemDisplayManager<string>>();
        display.Setup(manager => manager.BuildDisplayAsync(It.IsAny<string>(), updater, "SummaryAdminGrid"))
            .ReturnsAsync((string model, IUpdateModel _, string _) =>
            {
                var shape = new Shape();
                shape.Properties["Model"] = model;
                return shape;
            });
        using var services = new ServiceCollection()
            .AddSingleton(display.Object)
            .AddSingleton(Mock.Of<IUpdateModelAccessor>(accessor => accessor.ModelUpdater == updater))
            .BuildServiceProvider();

        var rows = await new AdminListRowBuilder(services)
            .BuildAsync(items, null, "SummaryAdminGrid", TestContext.Current.CancellationToken);

        Assert.Equal(items, rows.Select(row => row.GetProperty<string>("Model")));
        display.Verify(manager => manager.BuildDisplayAsync(It.IsAny<string>(), updater, "SummaryAdminGrid"), Times.Exactly(count));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public async Task BuildAsync_GenericContext_UsesBaseDisplayManagerIncludingEmptyCollections(int count)
    {
        var items = Enumerable.Repeat("item", count).ToArray();
        var context = new AdminListContext<object> { Source = items };
        var row = new Shape();
        var display = new Mock<IAdminListItemDisplayManager<object>>();
        display.Setup(manager => manager.BuildDisplayAsync("item", It.IsAny<IUpdateModel>(), "SummaryAdmin"))
            .ReturnsAsync(row);
        using var services = new ServiceCollection()
            .AddSingleton(display.Object)
            .AddSingleton(Mock.Of<IUpdateModelAccessor>())
            .BuildServiceProvider();

        var rows = await new AdminListRowBuilder(services)
            .BuildAsync(context.Source, context.ItemType, "SummaryAdmin", TestContext.Current.CancellationToken);

        Assert.Equal(count, rows.Count);
        Assert.All(rows, value => Assert.Same(row, value));
        display.Verify(manager => manager.BuildDisplayAsync("item", It.IsAny<IUpdateModel>(), "SummaryAdmin"), Times.Exactly(count));
    }

    [Fact]
    public async Task BuildAsync_UntypedItems_RequireExplicitType()
    {
        using var services = new ServiceCollection()
            .AddSingleton(Mock.Of<IAdminListItemDisplayManager<string>>())
            .AddSingleton(Mock.Of<IUpdateModelAccessor>())
            .BuildServiceProvider();
        var builder = new AdminListRowBuilder(services);
        var items = new ArrayList();

        await Assert.ThrowsAsync<ArgumentException>(() => builder.BuildAsync(items, null, "SummaryAdmin", TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<ArgumentException>(() => builder.BuildAsync(new AmbiguousItems(), null, "SummaryAdmin", TestContext.Current.CancellationToken));
        Assert.Empty(await builder.BuildAsync(items, typeof(string), "SummaryAdmin", TestContext.Current.CancellationToken));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(123)]
    public async Task BuildAsync_InvalidItem_Throws(object item)
    {
        using var services = new ServiceCollection()
            .AddSingleton(Mock.Of<IAdminListItemDisplayManager<string>>())
            .AddSingleton(Mock.Of<IUpdateModelAccessor>())
            .BuildServiceProvider();

        await Assert.ThrowsAsync<ArgumentException>(() => new AdminListRowBuilder(services)
            .BuildAsync(new ArrayList { item }, typeof(string), "SummaryAdmin", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task BuildAsync_CancellationBetweenRows_StopsRendering()
    {
        using var cancellation = new CancellationTokenSource();
        var display = new Mock<IAdminListItemDisplayManager<string>>();
        display.Setup(manager => manager.BuildDisplayAsync(It.IsAny<string>(), It.IsAny<IUpdateModel>(), "SummaryAdmin"))
            .ReturnsAsync(() =>
            {
                cancellation.Cancel();
                return new Shape();
            });
        using var services = new ServiceCollection()
            .AddSingleton(display.Object)
            .AddSingleton(Mock.Of<IUpdateModelAccessor>())
            .BuildServiceProvider();
        var builder = new AdminListRowBuilder(services);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            builder.BuildAsync(new[] { "one", "two" }, null, "SummaryAdmin", cancellation.Token));
        display.Verify(manager => manager.BuildDisplayAsync(It.IsAny<string>(), It.IsAny<IUpdateModel>(), "SummaryAdmin"), Times.Once);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            builder.BuildAsync(Array.Empty<string>(), null, "SummaryAdmin", cancellation.Token));
    }

    [Fact]
    public async Task BuildDisplayAsync_DefaultAdapter_ForwardsToGenericDisplayManager()
    {
        var updater = Mock.Of<IUpdateModel>();
        var row = new Shape();
        var display = new Mock<IDisplayManager<string>>();
        display.Setup(manager => manager.BuildDisplayAsync("item", updater, "SummaryAdminGrid", It.IsAny<string>()))
            .ReturnsAsync(row);

        Assert.Same(row, await new DefaultAdminListItemDisplayManager<string>(display.Object)
            .BuildDisplayAsync("item", updater, "SummaryAdminGrid"));
    }

    [Fact]
    public async Task BuildDisplayAsync_ContentAdapter_ForwardsToContentDisplayManager()
    {
        var item = new ContentItem();
        var updater = Mock.Of<IUpdateModel>();
        var row = new Shape();
        var display = new Mock<IContentItemDisplayManager>();
        display.Setup(manager => manager.BuildDisplayAsync(item, updater, "SummaryAdminGrid", It.IsAny<string>()))
            .ReturnsAsync(row);

        Assert.Same(row, await new ContentItemAdminListDisplayManager(display.Object)
            .BuildDisplayAsync(item, updater, "SummaryAdminGrid"));
    }

    [Theory]
    [InlineData("SummaryAdmin", true)]
    [InlineData("SummaryAdmin", false)]
    [InlineData("SummaryAdminGrid", false)]
    public async Task BuildDisplayAsync_ListPartAdapter_PreservesContextAndSuppressesSelection(string displayType, bool enableOrdering)
    {
        var updater = Mock.Of<IUpdateModel>();
        var entry = new ListPartAdminListEntry
        {
            ContentItem = new ContentItem(),
            EnableOrdering = enableOrdering,
            GroupId = "Group",
            Updater = updater,
        };
        var row = new ZoneHolding(() => ValueTask.FromResult<IShape>(new Shape()));
        row.Zones["Checkbox"] = new Shape();
        row.Zones["Select"] = new Shape();
        var sortable = new Shape();
        var factory = new Mock<IShapeFactory> { CallBase = true };
        factory.Setup(value => value.CreateAsync("ListPart_Sortable", It.IsAny<Func<ValueTask<IShape>>>(),
                It.IsAny<Action<ShapeCreatingContext>>(), It.IsAny<Action<ShapeCreatedContext>>()))
            .ReturnsAsync(sortable);
        var display = new Mock<IContentItemDisplayManager>();
        display.Setup(value => value.BuildDisplayAsync(entry.ContentItem, updater, displayType, "Group"))
            .ReturnsAsync(row);

        var result = await new ListPartAdminListDisplayManager(display.Object, factory.Object)
            .BuildDisplayAsync(entry, Mock.Of<IUpdateModel>(), displayType);

        Assert.Same(row, result);
        Assert.False(row.Zones.IsNotEmpty("Checkbox"));
        Assert.False(row.Zones.IsNotEmpty("Select"));
        Assert.Equal(enableOrdering, row.Zones.IsNotEmpty("Selectors"));
        if (enableOrdering)
        {
            Assert.Same(sortable, Assert.Single(row.Zones["Selectors"].Items));
        }

        display.Verify(value => value.BuildDisplayAsync(entry.ContentItem, updater, displayType, "Group"), Times.Once);
    }

    private sealed class AmbiguousItems : IEnumerable<string>, IEnumerable<int>
    {
        IEnumerator<string> IEnumerable<string>.GetEnumerator() => Enumerable.Empty<string>().GetEnumerator();
        IEnumerator<int> IEnumerable<int>.GetEnumerator() => Enumerable.Empty<int>().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<string>)this).GetEnumerator();
    }
}
