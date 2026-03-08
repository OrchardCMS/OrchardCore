using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.Tests.DisplayManagement;

public class AlternatesCollectionTests
{
    [Fact]
    public void Add_ShouldAddItem()
    {
        var collection = new AlternatesCollection();

        collection.Add("A");

        Assert.Equal(1, collection.Count);
        Assert.True(collection.Contains("A"));
    }

    [Fact]
    public void Add_ShouldIgnoreDuplicate()
    {
        var collection = new AlternatesCollection();

        collection.Add("A");
        collection.Add("A");

        Assert.Equal(1, collection.Count);
    }

    [Fact]
    public void Add_ShouldPreserveInsertionOrder()
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
    public void Add_ThrowsForNull()
    {
        var collection = new AlternatesCollection();

        Assert.Throws<ArgumentNullException>(() => collection.Add(null));
    }

    [Fact]
    public void Remove_ShouldRemoveItem()
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
    public void Remove_NonExistent_ShouldNotThrow()
    {
        var collection = new AlternatesCollection();

        collection.Add("A");
        collection.Remove("Z");

        Assert.Equal(1, collection.Count);
    }

    [Fact]
    public void Remove_OnEmpty_ShouldNotThrow()
    {
        var collection = new AlternatesCollection();

        collection.Remove("A");

        Assert.Equal(0, collection.Count);
    }

    [Fact]
    public void Remove_ThrowsForNull()
    {
        var collection = new AlternatesCollection();

        Assert.Throws<ArgumentNullException>(() => collection.Remove(null));
    }

    [Fact]
    public void Clear_ShouldRemoveAllItems()
    {
        var collection = new AlternatesCollection();

        collection.Add("A");
        collection.Add("B");
        collection.Clear();

        Assert.Equal(0, collection.Count);
        Assert.False(collection.Contains("A"));
    }

    [Fact]
    public void Clear_ShouldRemoveSegmentsAndCollectionItems()
    {
        var collection = new AlternatesCollection();

        collection.AddRange(new[] { "A", "B" });
        collection.Add("C");
        collection.Clear();

        Assert.Equal(0, collection.Count);
        Assert.False(collection.Contains("A"));
        Assert.False(collection.Contains("C"));
    }

    [Fact]
    public void Contains_ShouldFindItemsInCollection()
    {
        var collection = new AlternatesCollection();

        collection.Add("A");

        Assert.True(collection.Contains("A"));
        Assert.False(collection.Contains("B"));
    }

    [Fact]
    public void Contains_ShouldFindItemsInSegments()
    {
        var collection = new AlternatesCollection();

        collection.AddRange(new[] { "A", "B" });

        Assert.True(collection.Contains("A"));
        Assert.True(collection.Contains("B"));
        Assert.False(collection.Contains("C"));
    }

    [Fact]
    public void Contains_ShouldFindItemsAcrossSegmentsAndCollection()
    {
        var collection = new AlternatesCollection();

        collection.AddRange(new[] { "A" });
        collection.Add("B");

        Assert.True(collection.Contains("A"));
        Assert.True(collection.Contains("B"));
    }

    [Fact]
    public void Contains_ThrowsForNull()
    {
        var collection = new AlternatesCollection();

        Assert.Throws<ArgumentNullException>(() => collection.Contains(null));
    }

    [Fact]
    public void Count_ShouldReflectAllSources()
    {
        var collection = new AlternatesCollection();

        Assert.Equal(0, collection.Count);

        collection.Add("A");
        Assert.Equal(1, collection.Count);

        collection.AddRange(new[] { "B", "C" });
        Assert.Equal(3, collection.Count);
    }

    [Fact]
    public void Last_ShouldReturnEmptyForEmptyCollection()
    {
        var collection = new AlternatesCollection();

        Assert.Equal("", collection.Last);
    }

    [Fact]
    public void Last_ShouldReturnLastAddedItem()
    {
        var collection = new AlternatesCollection();

        collection.Add("A");
        collection.Add("B");

        Assert.Equal("B", collection.Last);
    }

    [Fact]
    public void Last_ShouldReturnLastSegmentItem_WhenNoCollectionItems()
    {
        var collection = new AlternatesCollection();

        collection.AddRange(new[] { "A", "B" });

        Assert.Equal("B", collection.Last);
    }

    [Fact]
    public void Last_ShouldReturnCollectionItem_WhenAddedAfterSegments()
    {
        var collection = new AlternatesCollection();

        collection.AddRange(new[] { "A", "B" });
        collection.Add("C");

        Assert.Equal("C", collection.Last);
    }

    [Fact]
    public void Last_WithMultipleSegments_ReturnsLastSegmentItem_WhenNoCollectionItems()
    {
        var collection = new AlternatesCollection();

        collection.AddRange(new[] { "A", "B" });
        collection.AddRange(new[] { "C", "D" });

        Assert.Equal("D", collection.Last);
    }

    [Fact]
    public void Indexer_ShouldReturnEmptyForOutOfRange()
    {
        var collection = new AlternatesCollection();

        Assert.Equal("", collection[0]);
        Assert.Equal("", collection[99]);
    }

    [Fact]
    public void AddRangeArray_ShouldStoreByReference()
    {
        // Segments should be accessible without materialization.
        var collection = new AlternatesCollection();
        var array = new[] { "A", "B", "C" };

        collection.AddRange(array);

        Assert.Equal(3, collection.Count);
        Assert.Equal("A", collection[0]);
        Assert.Equal("B", collection[1]);
        Assert.Equal("C", collection[2]);
    }

    [Fact]
    public void AddRangeArray_EmptyArray_ShouldBeIgnored()
    {
        var collection = new AlternatesCollection();

        collection.AddRange(Array.Empty<string>());

        Assert.Equal(0, collection.Count);
    }

    [Fact]
    public void AddRangeArray_MultipleCalls_ShouldAppend()
    {
        var collection = new AlternatesCollection();

        collection.AddRange(new[] { "A", "B" });
        collection.AddRange(new[] { "C", "D" });

        Assert.Equal(4, collection.Count);
        Assert.Equal("A", collection[0]);
        Assert.Equal("B", collection[1]);
        Assert.Equal("C", collection[2]);
        Assert.Equal("D", collection[3]);
    }

    [Fact]
    public void AddRangeArray_ThrowsForNull()
    {
        var collection = new AlternatesCollection();

        Assert.Throws<ArgumentNullException>(() => collection.AddRange((string[])null));
    }

    [Fact]
    public void AddRangeEnumerable_ShouldAddItems()
    {
        var collection = new AlternatesCollection();
        IEnumerable<string> items = new List<string> { "A", "B" };

        collection.AddRange(items);

        Assert.Equal(2, collection.Count);
        Assert.True(collection.Contains("A"));
        Assert.True(collection.Contains("B"));
    }

    [Fact]
    public void AddRangeEnumerable_ShouldIgnoreDuplicates()
    {
        var collection = new AlternatesCollection();

        collection.Add("A");
        collection.AddRange(new List<string> { "A", "B" } as IEnumerable<string>);

        Assert.Equal(2, collection.Count);
    }

    [Fact]
    public void AddRangeEnumerable_ThrowsForNull()
    {
        var collection = new AlternatesCollection();

        Assert.Throws<ArgumentNullException>(() => collection.AddRange((IEnumerable<string>)null));
    }

    [Fact]
    public void AddRangeAlternatesCollection_ShouldCopyAll()
    {
        var source = new AlternatesCollection();
        source.AddRange(new[] { "A", "B" });
        source.Add("C");

        var target = new AlternatesCollection();
        target.AddRange(source);

        Assert.Equal(3, target.Count);
        Assert.True(target.Contains("A"));
        Assert.True(target.Contains("B"));
        Assert.True(target.Contains("C"));
    }

    [Fact]
    public void Ordering_SegmentsComeBeforeCollectionItems()
    {
        var collection = new AlternatesCollection();

        // Segments are added first (lower priority).
        collection.AddRange(new[] { "Seg1", "Seg2" });

        // Collection items added after (higher priority).
        collection.Add("Col1");
        collection.Add("Col2");

        Assert.Equal("Seg1", collection[0]);
        Assert.Equal("Seg2", collection[1]);
        Assert.Equal("Col1", collection[2]);
        Assert.Equal("Col2", collection[3]);
    }

    [Fact]
    public void Ordering_MultipleSegmentsPreserveOrder()
    {
        var collection = new AlternatesCollection();

        collection.AddRange(new[] { "A1", "A2" });
        collection.AddRange(new[] { "B1", "B2" });
        collection.Add("C1");

        Assert.Equal("A1", collection[0]);
        Assert.Equal("A2", collection[1]);
        Assert.Equal("B1", collection[2]);
        Assert.Equal("B2", collection[3]);
        Assert.Equal("C1", collection[4]);
    }

    [Fact]
    public void Ordering_AddBeforeAndAfterSegments()
    {
        var collection = new AlternatesCollection();

        collection.Add("Early");

        // AddRange after Add copies into _collection to preserve call order.
        collection.AddRange(new[] { "Seg1", "Seg2" });

        collection.Add("Late");

        // Order matches the sequence of calls: Early, Seg1, Seg2, Late.
        Assert.Equal("Early", collection[0]);
        Assert.Equal("Seg1", collection[1]);
        Assert.Equal("Seg2", collection[2]);
        Assert.Equal("Late", collection[3]);
    }

    [Fact]
    public void Enumerate_ShouldMatchIndexer()
    {
        var collection = new AlternatesCollection();

        collection.AddRange(new[] { "A", "B" });
        collection.Add("C");
        collection.AddRange(new[] { "D", "E" });

        var list = collection.ToList();

        Assert.Equal(collection.Count, list.Count);

        for (var i = 0; i < list.Count; i++)
        {
            Assert.Equal(collection[i], list[i]);
        }
    }

    [Fact]
    public void Enumerate_EmptyCollection_ShouldYieldNothing()
    {
        var collection = new AlternatesCollection();

        Assert.Empty(collection.ToList());
    }

    [Fact]
    public void Remove_MaterializesSegments()
    {
        var collection = new AlternatesCollection();

        collection.AddRange(new[] { "A", "B", "C" });
        collection.Remove("B");

        Assert.Equal(2, collection.Count);
        Assert.True(collection.Contains("A"));
        Assert.False(collection.Contains("B"));
        Assert.True(collection.Contains("C"));
    }

    [Fact]
    public void Remove_AfterMixedAddAndAddRange_PreservesOrder()
    {
        var collection = new AlternatesCollection();

        collection.AddRange(new[] { "A", "B" });
        collection.Add("C");
        collection.Remove("B");

        Assert.Equal(2, collection.Count);

        // After materialization, order should be segments first, then collection items.
        Assert.Equal("A", collection[0]);
        Assert.Equal("C", collection[1]);
    }

    [Fact]
    public void Add_DoesNotMaterializeSegments()
    {
        var collection = new AlternatesCollection();
        var segment = new[] { "A", "B" };

        collection.AddRange(segment);
        collection.Add("C");

        // Segments should still be accessible by reference (not materialized).
        // Verify the count reflects both segments and collection.
        Assert.Equal(3, collection.Count);

        // Verify segment order is preserved: segments first, then collection.
        Assert.Equal("A", collection[0]);
        Assert.Equal("B", collection[1]);
        Assert.Equal("C", collection[2]);
    }

    [Fact]
    public void Add_DuplicateFromSegment_ShouldBeIgnored()
    {
        var collection = new AlternatesCollection();

        collection.AddRange(new[] { "A", "B" });
        collection.Add("A");

        Assert.Equal(2, collection.Count);
    }

    [Fact]
    public void Constructor_WithParams_ShouldAddItems()
    {
        var collection = new AlternatesCollection("A", "B", "C");

        Assert.Equal(3, collection.Count);
        Assert.Equal("A", collection[0]);
        Assert.Equal("B", collection[1]);
        Assert.Equal("C", collection[2]);
    }

    [Fact]
    public void Constructor_WithDuplicateParams_ShouldDeduplicate()
    {
        var collection = new AlternatesCollection("A", "B", "A");

        Assert.Equal(2, collection.Count);
    }

    [Fact]
    public void Constructor_WithNullArray_DoesNotThrow()
    {
        var collection = new AlternatesCollection(null);

        Assert.Equal(0, collection.Count);
    }

    [Fact]
    public void Empty_ShouldNotAllowMutation()
    {
        Assert.Throws<NotSupportedException>(() => AlternatesCollection.Empty.Add("A"));
        Assert.Throws<NotSupportedException>(() => AlternatesCollection.Empty.AddRange(new[] { "A" }));
        Assert.Throws<NotSupportedException>(() => AlternatesCollection.Empty.AddRange(new List<string> { "A" } as IEnumerable<string>));

        // Clear and Remove on an empty collection are no-ops, so they don't throw.
        AlternatesCollection.Empty.Clear();
        AlternatesCollection.Empty.Remove("A");
    }

    [Fact]
    public void Empty_ShouldHaveZeroCount()
    {
        Assert.Equal(0, AlternatesCollection.Empty.Count);
        Assert.Equal("", AlternatesCollection.Empty.Last);
        Assert.Empty(AlternatesCollection.Empty.ToList());
    }

    [Fact]
    public void Empty_IsReadOnly()
    {
        // Verify that Empty is truly immutable
        var empty = AlternatesCollection.Empty;

        Assert.Equal(0, empty.Count);
        Assert.Equal("", empty.Last);
        Assert.False(empty.Contains("test"));
    }

    [Fact]
    public void Clear_ThenAdd_ShouldWork()
    {
        var collection = new AlternatesCollection();

        collection.AddRange(new[] { "A", "B" });
        collection.Add("C");
        collection.Clear();

        collection.Add("D");
        collection.AddRange(new[] { "E" });

        // After clear, Add("D") creates _collection=[D].
        // AddRange(["E"]) copies into _collection since it has items: [D, E].
        Assert.Equal(2, collection.Count);
        Assert.Equal("D", collection[0]);
        Assert.Equal("E", collection[1]);
    }

    [Fact]
    public void Enumerate_AfterRemove_ShouldReflectChanges()
    {
        var collection = new AlternatesCollection();

        collection.AddRange(new[] { "A", "B", "C" });
        collection.Add("D");
        collection.Remove("B");

        var list = collection.ToList();

        Assert.Equal(3, list.Count);
        Assert.Contains("A", list);
        Assert.Contains("C", list);
        Assert.Contains("D", list);
        Assert.DoesNotContain("B", list);
    }

    [Fact]
    public void Indexer_AfterRemove_ShouldReflectMaterializedOrder()
    {
        var collection = new AlternatesCollection();

        collection.AddRange(new[] { "A", "B" });
        collection.Add("C");
        collection.Remove("A");

        // After materialization: segments (A, B) + collection (C) → materialized as B, C.
        Assert.Equal(2, collection.Count);
        Assert.Equal("B", collection[0]);
        Assert.Equal("C", collection[1]);
    }

    [Fact]
    public void AddRangeAlternatesCollection_PreservesSourceOrdering()
    {
        var source = new AlternatesCollection();
        source.AddRange(new[] { "S1", "S2" });
        source.Add("C1");

        var target = new AlternatesCollection();
        target.Add("T1");
        target.AddRange(source);

        // target: Add("T1") → _collection=[T1].
        // AddRange(source) copies source segments (S1, S2) and source collection (C1)
        // into target's _collection to maintain call order: T1, S1, S2, C1.
        Assert.Equal(4, target.Count);
        Assert.Equal("T1", target[0]);
        Assert.Equal("S1", target[1]);
        Assert.Equal("S2", target[2]);
        Assert.Equal("C1", target[3]);
    }

    [Fact]
    public void AddRangeAlternatesCollection_Empty_DoesNothing()
    {
        var source = new AlternatesCollection();
        var target = new AlternatesCollection();

        target.Add("A");
        target.AddRange(source);

        Assert.Equal(1, target.Count);
        Assert.Equal("A", target[0]);
    }

    [Fact]
    public void Last_AfterClear_ShouldReturnEmpty()
    {
        var collection = new AlternatesCollection();

        collection.Add("A");
        collection.Clear();

        Assert.Equal("", collection.Last);
    }

    [Fact]
    public void Last_WithSingleItem_ReturnsItem()
    {
        var collection = new AlternatesCollection();

        collection.Add("Only");

        Assert.Equal("Only", collection.Last);
    }

    [Fact]
    public void Contains_AfterRemove_ShouldNotFindRemovedItem()
    {
        var collection = new AlternatesCollection();

        collection.AddRange(new[] { "A", "B" });
        collection.Remove("A");

        Assert.False(collection.Contains("A"));
        Assert.True(collection.Contains("B"));
    }

    [Fact]
    public void AddRangeArray_AfterAdd_CopiesIntoCollectionToPreserveOrder()
    {
        var collection = new AlternatesCollection();

        collection.Add("X");
        collection.AddRange(new[] { "A", "B" });
        collection.Add("Y");

        // Strict call order: X, A, B, Y.
        Assert.Equal(4, collection.Count);
        Assert.Equal("X", collection[0]);
        Assert.Equal("A", collection[1]);
        Assert.Equal("B", collection[2]);
        Assert.Equal("Y", collection[3]);
        Assert.Equal("Y", collection.Last);
    }

    [Fact]
    public void AddRangeArray_AfterAdd_DeduplicatesWhenCopying()
    {
        var collection = new AlternatesCollection();

        collection.Add("A");
        collection.AddRange(new[] { "A", "B", "C" });

        Assert.Equal(3, collection.Count);
        Assert.True(collection.Contains("A"));
        Assert.True(collection.Contains("B"));
        Assert.True(collection.Contains("C"));
    }

    [Fact]
    public void AddRangeArray_WithDuplicatesInArray_FallsBackToItemByItem()
    {
        var collection = new AlternatesCollection();

        collection.Add("B");
        collection.AddRange(new[] { "A", "B", "C" });

        Assert.Equal(3, collection.Count);
        Assert.True(collection.Contains("A"));
        Assert.True(collection.Contains("B"));
        Assert.True(collection.Contains("C"));
    }

    [Fact]
    public void AddRangeArray_BeforeAdd_StoresAsSegment()
    {
        var collection = new AlternatesCollection();

        collection.AddRange(new[] { "A", "B" });
        collection.AddRange(new[] { "C", "D" });
        collection.Add("E");

        // Segments stored by reference, then collection item after.
        Assert.Equal(5, collection.Count);
        Assert.Equal("A", collection[0]);
        Assert.Equal("B", collection[1]);
        Assert.Equal("C", collection[2]);
        Assert.Equal("D", collection[3]);
        Assert.Equal("E", collection[4]);
    }

    [Fact]
    public void Ordering_InterleavedAddAndAddRange_PreservesCallOrder()
    {
        var collection = new AlternatesCollection();

        collection.Add("1");
        collection.AddRange(new[] { "2", "3" });
        collection.Add("4");
        collection.AddRange(new[] { "5" });
        collection.Add("6");

        Assert.Equal(6, collection.Count);
        for (var i = 0; i < 6; i++)
        {
            Assert.Equal((i + 1).ToString(), collection[i]);
        }
    }

    [Fact]
    public void Ordering_InterleavedAddAndAddRange_EnumerationMatchesCallOrder()
    {
        var collection = new AlternatesCollection();

        collection.Add("1");
        collection.AddRange(new[] { "2", "3" });
        collection.Add("4");
        collection.AddRange(new[] { "5" });
        collection.Add("6");

        var list = collection.ToList();

        Assert.Equal(6, list.Count);
        for (var i = 0; i < 6; i++)
        {
            Assert.Equal((i + 1).ToString(), list[i]);
        }
    }

    [Fact]
    public void Last_AfterInterleavedAddAndAddRange_ReturnsLastItem()
    {
        var collection = new AlternatesCollection();

        collection.AddRange(new[] { "A" });
        collection.Add("B");
        collection.AddRange(new[] { "C" });

        Assert.Equal("C", collection.Last);
    }

    [Fact]
    public void Remove_FromMiddleOfSegment_PreservesOrder()
    {
        var collection = new AlternatesCollection();

        collection.AddRange(new[] { "A", "B", "C", "D" });
        collection.Remove("B");

        Assert.Equal(3, collection.Count);
        Assert.Equal("A", collection[0]);
        Assert.Equal("C", collection[1]);
        Assert.Equal("D", collection[2]);
    }

    [Fact]
    public void Remove_FromAdditionalSegment_Works()
    {
        var collection = new AlternatesCollection();

        collection.AddRange(new[] { "A", "B" });
        collection.AddRange(new[] { "C", "D" });
        collection.Remove("C");

        Assert.Equal(3, collection.Count);
        Assert.Equal("A", collection[0]);
        Assert.Equal("B", collection[1]);
        Assert.Equal("D", collection[2]);
    }

    [Fact]
    public void Remove_EntireSegment_PromotesNextSegment()
    {
        var collection = new AlternatesCollection();

        collection.AddRange(new[] { "A" });
        collection.AddRange(new[] { "B", "C" });
        collection.Remove("A");

        Assert.Equal(2, collection.Count);
        Assert.Equal("B", collection[0]);
        Assert.Equal("C", collection[1]);
    }

    [Fact]
    public void Remove_EntireAdditionalSegment_RemovesSegment()
    {
        var collection = new AlternatesCollection();

        collection.AddRange(new[] { "A", "B" });
        collection.AddRange(new[] { "C" });
        collection.AddRange(new[] { "D", "E" });
        collection.Remove("C");

        Assert.Equal(4, collection.Count);
        Assert.Equal("A", collection[0]);
        Assert.Equal("B", collection[1]);
        Assert.Equal("D", collection[2]);
        Assert.Equal("E", collection[3]);
    }

    [Fact]
    public void Count_AfterRemove_IsCorrect()
    {
        var collection = new AlternatesCollection();

        collection.AddRange(new[] { "A", "B", "C" });
        Assert.Equal(3, collection.Count);

        collection.Remove("B");
        Assert.Equal(2, collection.Count);

        collection.Remove("A");
        Assert.Equal(1, collection.Count);

        collection.Remove("C");
        Assert.Equal(0, collection.Count);
    }

    [Fact]
    public void Count_AfterClear_IsZero()
    {
        var collection = new AlternatesCollection();

        collection.AddRange(new[] { "A", "B" });
        collection.Add("C");

        collection.Clear();

        Assert.Equal(0, collection.Count);
    }

    [Fact]
    public void Count_DuringComplexOperations_RemainsAccurate()
    {
        var collection = new AlternatesCollection();

        Assert.Equal(0, collection.Count);

        collection.Add("A");
        Assert.Equal(1, collection.Count);

        collection.AddRange(new[] { "B", "C" });
        Assert.Equal(3, collection.Count);

        collection.Add("D");
        Assert.Equal(4, collection.Count);

        collection.Remove("B");
        Assert.Equal(3, collection.Count);

        collection.AddRange(new[] { "E", "F", "G" });
        Assert.Equal(6, collection.Count);

        collection.Clear();
        Assert.Equal(0, collection.Count);
    }

    [Fact]
    public void Indexer_LargeIndex_ReturnsEmpty()
    {
        var collection = new AlternatesCollection();

        collection.AddRange(new[] { "A", "B" });

        Assert.Equal("", collection[1000]);
    }

    [Fact]
    public void Enumerate_AfterMultipleOperations_ReturnsCorrectOrder()
    {
        var collection = new AlternatesCollection();

        collection.AddRange(new[] { "A", "B" });
        collection.Add("C");
        collection.Remove("B");
        collection.AddRange(new[] { "D" });
        collection.Add("E");

        var list = collection.ToList();

        Assert.Equal(4, list.Count);
        Assert.Equal("A", list[0]);
        Assert.Equal("C", list[1]);
        Assert.Equal("D", list[2]);
        Assert.Equal("E", list[3]);
    }

    [Fact]
    public void Indexer_SpansMultipleSegments_ReturnsCorrectItems()
    {
        var collection = new AlternatesCollection();

        collection.AddRange(new[] { "A", "B" }); // segment 0
        collection.AddRange(new[] { "C", "D", "E" }); // segment 1
        collection.AddRange(new[] { "F" }); // segment 2
        collection.Add("G"); // items

        Assert.Equal("A", collection[0]);
        Assert.Equal("B", collection[1]);
        Assert.Equal("C", collection[2]);
        Assert.Equal("D", collection[3]);
        Assert.Equal("E", collection[4]);
        Assert.Equal("F", collection[5]);
        Assert.Equal("G", collection[6]);
    }

    [Fact]
    public void FlushItems_PreservesOrderWhenAddingSegment()
    {
        var collection = new AlternatesCollection();

        collection.Add("X");
        collection.Add("Y");
        collection.AddRange(new[] { "A", "B" });

        // Items X, Y should be flushed as a segment before A, B segment is added
        Assert.Equal("X", collection[0]);
        Assert.Equal("Y", collection[1]);
        Assert.Equal("A", collection[2]);
        Assert.Equal("B", collection[3]);
    }

    [Fact]
    public void AddRange_MultipleEmptyArrays_DoesNotAffectCollection()
    {
        var collection = new AlternatesCollection();

        collection.Add("A");
        collection.AddRange(Array.Empty<string>());
        collection.AddRange(Array.Empty<string>());
        collection.Add("B");

        Assert.Equal(2, collection.Count);
        Assert.Equal("A", collection[0]);
        Assert.Equal("B", collection[1]);
    }
}
