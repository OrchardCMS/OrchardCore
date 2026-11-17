using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Zones;

namespace OrchardCore.Tests.DisplayManagement.Zones;

public class PositionedListExtensionsTests
{
    [Theory]
    [InlineData(3)]
    [InlineData(16)]
    [InlineData(17)]
    [InlineData(50)]
    public void SortByPosition_ItemsOutOfOrder_SortsByPosition(int count)
    {
        var items = new List<IPositioned>();

        // Added in reverse order, with the positions 'count', 'count - 1', ... '1'.
        for (var i = count; i >= 1; i--)
        {
            items.Add(new Shape { Position = i.ToString() });
        }

        items.SortByPosition();

        for (var i = 0; i < count; i++)
        {
            Assert.Equal((i + 1).ToString(), items[i].Position);
        }
    }

    [Theory]
    [InlineData(6)]
    [InlineData(40)]
    public void SortByPosition_EqualPositions_KeepsTheInsertionOrder(int count)
    {
        var items = new List<IPositioned>();

        // Half the items share the position '2', the others '1', interleaved: 2, 1, 2, 1, ...
        for (var i = 0; i < count; i++)
        {
            items.Add(new Shape { Position = i % 2 == 0 ? "2" : "1", Id = i.ToString() });
        }

        items.SortByPosition();

        // The '1' items come first, each group in insertion order.
        var ids = items.Select(item => int.Parse(((Shape)item).Id)).ToArray();

        Assert.Equal(Enumerable.Range(0, count).Where(i => i % 2 == 1), ids.Take(count / 2));
        Assert.Equal(Enumerable.Range(0, count).Where(i => i % 2 == 0), ids.Skip(count / 2));
    }

    [Fact]
    public void SortByPosition_AlreadySorted_KeepsTheItems()
    {
        var first = new Shape { Position = "1" };
        var second = new Shape { Position = "1.5" };
        var third = new Shape { Position = "after" };
        var items = new List<IPositioned> { first, second, third };

        items.SortByPosition();

        Assert.Equal([first, second, third], items);
    }

    [Fact]
    public async Task Items_AddedOutOfOrder_AreReadSorted()
    {
        var shape = new Shape();
        var late = new Shape { Position = "10" };
        var early = new Shape { Position = "2" };
        var before = new Shape();

        await shape.AddAsync(late, "10");
        await shape.AddAsync(early, "2");
        await shape.AddAsync(before, "before");

        Assert.Equal([before, early, late], shape.Items);

        // The list returned by Items is the sorted one, and stays sorted when read again.
        Assert.Equal([before, early, late], shape.Items);
    }
}
