using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.Tests.DisplayManagement;

public class AlternatesCollectionTests
{
    [Fact]
    public void Add_Default_AddsItem()
    {
        var collection = new AlternatesCollection();

        collection.Add("A");

        Assert.Equal(1, collection.Count);
        Assert.True(collection.Contains("A"));
    }

    [Fact]
    public void Add_Default_IgnoresDuplicate()
    {
        var collection = new AlternatesCollection();

        collection.Add("A");
        collection.Add("A");

        Assert.Equal(1, collection.Count);
    }

    [Fact]
    public void Add_Default_PreservesInsertionOrder()
    {
        var collection = new AlternatesCollection();

        collection.Add("A");
        collection.Add("B");
        collection.Add("C");

        Assert.Equal("A", collection[0]);
        Assert.Equal("B", collection[1]);
        Assert.Equal("C", collection[2]);
    }

    [Fact]
    public void Add_Default_ThrowsForNull()
    {
        var collection = new AlternatesCollection();

        Assert.Throws<ArgumentNullException>(() => collection.Add(null));
    }

    [Fact]
    public void Remove_Default_RemovesItem()
    {
        var collection = new AlternatesCollection();

        collection.Add("A");
        collection.Add("B");
        collection.Remove("A");

        Assert.Equal(1, collection.Count);
        Assert.False(collection.Contains("A"));
        Assert.True(collection.Contains("B"));
    }

    [Fact]
    public void Remove_NonExistent_DoesNotThrow()
    {
        var collection = new AlternatesCollection();

        collection.Add("A");
        collection.Remove("Z");

        Assert.Equal(1, collection.Count);
    }

    [Fact]
    public void Remove_Empty_DoesNotThrow()
    {
        var collection = new AlternatesCollection();

        collection.Remove("A");

        Assert.Equal(0, collection.Count);
    }

    [Fact]
    public void Remove_Default_ThrowsForNull()
    {
        var collection = new AlternatesCollection();

        Assert.Throws<ArgumentNullException>(() => collection.Remove(null));
    }

    [Fact]
    public void Remove_Default_PreservesOrderOfRemainingItems()
    {
        var collection = new AlternatesCollection();

        collection.Add("A");
        collection.Add("B");
        collection.Add("C");
        collection.Remove("B");

        Assert.Equal(2, collection.Count);
        Assert.Equal("A", collection[0]);
        Assert.Equal("C", collection[1]);
    }

    [Fact]
    public void Clear_Default_RemovesAllItems()
    {
        var collection = new AlternatesCollection();

        collection.Add("A");
        collection.Add("B");
        collection.Clear();

        Assert.Equal(0, collection.Count);
        Assert.False(collection.Contains("A"));
    }

    [Fact]
    public void Contains_Default_FindItems()
    {
        var collection = new AlternatesCollection();

        collection.Add("A");

        Assert.True(collection.Contains("A"));
        Assert.False(collection.Contains("B"));
    }

    [Fact]
    public void Contains_Default_ThrowsForNull()
    {
        var collection = new AlternatesCollection();

        Assert.Throws<ArgumentNullException>(() => collection.Contains(null));
    }

    [Fact]
    public void Count_Default_ReflectCurrentState()
    {
        var collection = new AlternatesCollection();

        Assert.Equal(0, collection.Count);

        collection.Add("A");
        Assert.Equal(1, collection.Count);

        collection.Add("B");
        Assert.Equal(2, collection.Count);

        collection.Remove("A");
        Assert.Equal(1, collection.Count);

        collection.Clear();
        Assert.Equal(0, collection.Count);
    }

    [Fact]
    public void Last_Default_ReturnsEmptyForEmptyCollection()
    {
        var collection = new AlternatesCollection();

        Assert.Equal("", collection.Last);
    }

    [Fact]
    public void Last_Default_ReturnsLastAddedItem()
    {
        var collection = new AlternatesCollection();

        collection.Add("A");
        collection.Add("B");

        Assert.Equal("B", collection.Last);
    }

    [Fact]
    public void Last_AfterRemove_ReturnsNewLast()
    {
        var collection = new AlternatesCollection();

        collection.Add("A");
        collection.Add("B");
        collection.Add("C");
        collection.Remove("C");

        Assert.Equal("B", collection.Last);
    }

    [Fact]
    public void Indexer_Default_ReturnsEmptyForOutOfRange()
    {
        var collection = new AlternatesCollection();

        Assert.Equal("", collection[0]);
        Assert.Equal("", collection[99]);
    }

    [Fact]
    public void Indexer_Default_ReturnsCorrectItems()
    {
        var collection = new AlternatesCollection();

        collection.Add("A");
        collection.Add("B");
        collection.Add("C");

        Assert.Equal("A", collection[0]);
        Assert.Equal("B", collection[1]);
        Assert.Equal("C", collection[2]);
    }

    [Fact]
    public void AddRangeArray_Default_AddsItems()
    {
        var collection = new AlternatesCollection();
        var array = new[] { "A", "B", "C" };

        collection.AddRange(array);

        Assert.Equal(3, collection.Count);
        Assert.Equal("A", collection[0]);
        Assert.Equal("B", collection[1]);
        Assert.Equal("C", collection[2]);
    }

    [Fact]
    public void AddRangeArray_EmptyArray_IsIgnored()
    {
        var collection = new AlternatesCollection();

        collection.AddRange(Array.Empty<string>());

        Assert.Equal(0, collection.Count);
    }

    [Fact]
    public void AddRangeArray_Duplicates_IgnoresThem()
    {
        var collection = new AlternatesCollection();

        collection.Add("B");
        collection.AddRange(new[] { "A", "B", "C" });

        Assert.Equal(3, collection.Count);
        Assert.Equal("B", collection[0]);
        Assert.Equal("A", collection[1]);
        Assert.Equal("C", collection[2]);
    }

    [Fact]
    public void AddRangeEnumerable_Default_AddsItems()
    {
        var collection = new AlternatesCollection();
        IEnumerable<string> items = new List<string> { "A", "B" };

        collection.AddRange(items);

        Assert.Equal(2, collection.Count);
        Assert.True(collection.Contains("A"));
        Assert.True(collection.Contains("B"));
    }

    [Fact]
    public void AddRangeEnumerable_Default_IgnoresDuplicates()
    {
        var collection = new AlternatesCollection();

        collection.Add("A");
        collection.AddRange(new List<string> { "A", "B" } as IEnumerable<string>);

        Assert.Equal(2, collection.Count);
    }

    [Fact]
    public void AddRangeEnumerable_Default_ThrowsForNull()
    {
        var collection = new AlternatesCollection();

        Assert.Throws<ArgumentNullException>(() => collection.AddRange((IEnumerable<string>)null));
    }

    [Fact]
    public void AddRangeAlternatesCollection_Default_CopyAll()
    {
        var source = new AlternatesCollection();
        source.Add("A");
        source.Add("B");
        source.Add("C");

        var target = new AlternatesCollection();
        target.AddRange(source);

        Assert.Equal(3, target.Count);
        Assert.True(target.Contains("A"));
        Assert.True(target.Contains("B"));
        Assert.True(target.Contains("C"));
    }

    [Fact]
    public void AddRangeAlternatesCollection_Default_IgnoresDuplicates()
    {
        var source = new AlternatesCollection();
        source.Add("A");
        source.Add("B");
        source.Add("C");

        var target = new AlternatesCollection();
        target.Add("B");
        target.AddRange(source);

        Assert.Equal(3, target.Count);
        Assert.Equal("B", target[0]);
        Assert.Equal("A", target[1]);
        Assert.Equal("C", target[2]);
    }

    [Fact]
    public void AddRangeAlternatesCollection_Default_ThrowsForNull()
    {
        var collection = new AlternatesCollection();

        Assert.Throws<ArgumentNullException>(() => collection.AddRange((AlternatesCollection)null));
    }

    [Fact]
    public void Enumerate_Default_MatchIndexer()
    {
        var collection = new AlternatesCollection();

        collection.Add("A");
        collection.Add("B");
        collection.Add("C");

        var list = collection.ToList();

        Assert.Equal(collection.Count, list.Count);

        for (var i = 0; i < list.Count; i++)
        {
            Assert.Equal(collection[i], list[i]);
        }
    }

    [Fact]
    public void Enumerate_EmptyCollection_YieldNothing()
    {
        var collection = new AlternatesCollection();

        Assert.Empty(collection.ToList());
    }

    [Fact]
    public void Enumerate_Default_PreservesInsertionOrder()
    {
        var collection = new AlternatesCollection();

        collection.Add("First");
        collection.Add("Second");
        collection.Add("Third");

        var list = collection.ToList();

        Assert.Equal("First", list[0]);
        Assert.Equal("Second", list[1]);
        Assert.Equal("Third", list[2]);
    }

    [Fact]
    public void Constructor_Params_AddsItems()
    {
        var collection = new AlternatesCollection("A", "B", "C");

        Assert.Equal(3, collection.Count);
        Assert.Equal("A", collection[0]);
        Assert.Equal("B", collection[1]);
        Assert.Equal("C", collection[2]);
    }

    [Fact]
    public void Constructor_DuplicateParams_Deduplicate()
    {
        var collection = new AlternatesCollection("A", "B", "A");

        Assert.Equal(2, collection.Count);
        Assert.Equal("A", collection[0]);
        Assert.Equal("B", collection[1]);
    }

    [Fact]
    public void Constructor_NullArray_DoesNotThrow()
    {
        var collection = new AlternatesCollection(null);

        Assert.Equal(0, collection.Count);
    }

    [Fact]
    public void Empty_Default_NotAllowMutation()
    {
        Assert.Throws<NotSupportedException>(() => AlternatesCollection.Empty.Add("A"));
        Assert.Throws<NotSupportedException>(() => AlternatesCollection.Empty.AddRange(new[] { "A" }));
        Assert.Throws<NotSupportedException>(() => AlternatesCollection.Empty.AddRange(new List<string> { "A" } as IEnumerable<string>));

        // Clear and Remove on an empty collection are no-ops, so they don't throw.
        AlternatesCollection.Empty.Clear();
        AlternatesCollection.Empty.Remove("A");
    }

    [Fact]
    public void Empty_Default_HasZeroCount()
    {
        Assert.Equal(0, AlternatesCollection.Empty.Count);
        Assert.Equal("", AlternatesCollection.Empty.Last);
        Assert.Empty(AlternatesCollection.Empty.ToList());
    }

    [Fact]
    public void Empty_Default_IsReadOnly()
    {
        var empty = AlternatesCollection.Empty;

        Assert.Equal(0, empty.Count);
        Assert.Equal("", empty.Last);
        Assert.False(empty.Contains("test"));
    }

    [Fact]
    public void Clear_ThenAdd_Works()
    {
        var collection = new AlternatesCollection();

        collection.Add("A");
        collection.Add("B");
        collection.Clear();

        collection.Add("C");
        collection.Add("D");

        Assert.Equal(2, collection.Count);
        Assert.Equal("C", collection[0]);
        Assert.Equal("D", collection[1]);
    }

    [Fact]
    public void MixedOperations_MaintainConsistency_Succeeds()
    {
        var collection = new AlternatesCollection();

        collection.Add("A");
        collection.AddRange(new[] { "B", "C" });
        collection.Add("D");
        collection.Remove("B");
        collection.AddRange(new[] { "E", "F" });

        Assert.Equal(5, collection.Count);
        Assert.Equal("A", collection[0]);
        Assert.Equal("C", collection[1]);
        Assert.Equal("D", collection[2]);
        Assert.Equal("E", collection[3]);
        Assert.Equal("F", collection[4]);

        var list = collection.ToList();
        Assert.Equal(5, list.Count);
        Assert.Equal("A", list[0]);
        Assert.Equal("C", list[1]);
        Assert.Equal("D", list[2]);
        Assert.Equal("E", list[3]);
        Assert.Equal("F", list[4]);
    }

    [Fact]
    public void Last_AfterClear_ReturnsEmpty()
    {
        var collection = new AlternatesCollection();

        collection.Add("A");
        collection.Clear();

        Assert.Equal("", collection.Last);
    }

    [Fact]
    public void DuplicatesAcrossMultipleOperations_Default_AreIgnored()
    {
        var collection = new AlternatesCollection();

        collection.Add("A");
        collection.Add("B");
        collection.AddRange(new[] { "B", "C", "A" });
        collection.Add("D");
        collection.AddRange(new[] { "D", "E" });

        Assert.Equal(5, collection.Count);
        Assert.True(collection.Contains("A"));
        Assert.True(collection.Contains("B"));
        Assert.True(collection.Contains("C"));
        Assert.True(collection.Contains("D"));
        Assert.True(collection.Contains("E"));

        // Verify order: first occurrence wins
        Assert.Equal("A", collection[0]);
        Assert.Equal("B", collection[1]);
        Assert.Equal("C", collection[2]);
        Assert.Equal("D", collection[3]);
        Assert.Equal("E", collection[4]);
    }

    [Fact]
    public void Remove_MultipleTimes_WorksCorrectly()
    {
        var collection = new AlternatesCollection();

        collection.Add("A");
        collection.Add("B");
        collection.Add("C");

        collection.Remove("B");
        collection.Remove("B"); // Second remove should be no-op

        Assert.Equal(2, collection.Count);
        Assert.False(collection.Contains("B"));
    }

    [Fact]
    public void AddRange_Default_PreservesOrderFromSourceCollection()
    {
        var source = new AlternatesCollection();
        source.Add("Z");
        source.Add("Y");
        source.Add("X");

        var target = new AlternatesCollection();
        target.AddRange(source);

        Assert.Equal("Z", target[0]);
        Assert.Equal("Y", target[1]);
        Assert.Equal("X", target[2]);
    }
}
